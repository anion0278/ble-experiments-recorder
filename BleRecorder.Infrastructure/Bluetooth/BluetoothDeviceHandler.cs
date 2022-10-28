using System.Diagnostics;
using System.Security;
using System.Text;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using System.Threading;

namespace BleRecorder.Infrastructure.Bluetooth;

public class BluetoothDeviceHandler : IBluetoothDeviceHandler
{
    private readonly IDateTimeService _dateTimeService;
    private readonly ISynchronizationContextProvider _contextProvider;
    public event EventHandler<string>? DataReceived;
    public event EventHandler? DeviceStatusChanged;

    private BluetoothLEDevice? _device;
    private GattCharacteristic? _rxCharacteristic;
    private GattCharacteristic? _txCharacteristic;
    private readonly System.Timers.Timer _incomingHeartbeatWatchdog; // thread-safe timer
    private readonly System.Timers.Timer _outgoingHeartbeat; // thread-safe timer
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(0.4);
    private readonly TimeSpan _incomingHeartbeatTimeout = TimeSpan.FromSeconds(1.0);
    private bool _isConnected;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1);
    private readonly object _disposingSyncRoot = new();

    public bool IsDisposed { get; private set; }
    public ulong Address { get; set; }
    public string Name { get; set; }
    public short SignalStrength { get; set; }
    public DateTimeOffset LatestTimestamp { get; set; }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (value == _isConnected) return;
            _isConnected = value;
            DeviceStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BluetoothDeviceHandler(IDateTimeService dateTimeService,
        ISynchronizationContextProvider contextProvider,
        string name, ulong address, short signalStrength, DateTimeOffset timestamp)
    {
        _dateTimeService = dateTimeService;
        _contextProvider = contextProvider;
        Name = name;
        Address = address;
        SignalStrength = signalStrength;
        LatestTimestamp = timestamp;
        _incomingHeartbeatWatchdog = new System.Timers.Timer(_heartbeatInterval.TotalMilliseconds);
        _incomingHeartbeatWatchdog.AutoReset = false; // requires manual enabling after each Elapsed event fired
        _incomingHeartbeatWatchdog.Elapsed += OnWatchdogPeriodElapsed;

        _outgoingHeartbeat = new System.Timers.Timer(_heartbeatInterval.TotalMilliseconds);
        _outgoingHeartbeat.Elapsed += OnHeartbeatPeriodElapsed;
    }

    private async void OnHeartbeatPeriodElapsed(object? sender, ElapsedEventArgs e)
    {
        await SendAsync("Heartbeat");
    }

    public async Task<IBluetoothDeviceHandler> ConnectDeviceAsync()
    {
        if (IsDisposed) throw new ObjectDisposedException("BLE devise was disposed! Create a new object.");

        _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
        _device.ConnectionStatusChanged += BleDeviceOnConnectionStatusChanged;

        //DeviceInformation.Pairing.IsPaired;
        //var y = bleDevice.DeviceInformation.Pairing.CanPair;

        //var allDeviceServices = (await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached)).Services.ToArray();

        var uartGuid = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        var uartService = (await _device.GetGattServicesForUuidAsync(uartGuid)).Services.Single();

        if ((await _device.GetGattServicesForUuidAsync(uartGuid)).Status != GattCommunicationStatus.Success) throw new Exception("Connection with BLE device failed!");

        var rxId = new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        var txId = new Guid("6e400002-b5a3-f393-e0a9-e50e24dcca9e");

        var uartCharacteristics = (await uartService.GetCharacteristicsAsync()).Characteristics;
        _rxCharacteristic = uartCharacteristics.Single(s => s.Uuid == rxId);
        _txCharacteristic = uartCharacteristics.Single(s => s.Uuid == txId);

        // TODO try GattClientCharacteristicConfigurationDescriptorValue.Indicate - indicate requires the client to acnknowhandde succ.transmition
        await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify); // required to receive data
        _rxCharacteristic.ValueChanged += Uart_ReceivedData;

        _incomingHeartbeatWatchdog.Start();
        _outgoingHeartbeat.Start();

        // var isAccessAllowed = await _device.RequestAccessAsync();

        return this;
    }

    private void OnWatchdogPeriodElapsed(object? sender, ElapsedEventArgs e)
    {
        var ts = _dateTimeService.Now;
        if (ts - LatestTimestamp < _incomingHeartbeatTimeout)
        {
            _incomingHeartbeatWatchdog.Enabled = true; // + AutoReset=false ensures that only a single thread at a time takes care of heartbeat
            return;
        }

        Disconnect();

        _contextProvider.RunInContext(() => throw new DeviceHeartbeatTimeoutException());
    }

    private void BleDeviceOnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        switch (sender.ConnectionStatus) // we cannot rely on this prop, since its not updated when the device is disconnected "externally"
        {
            case BluetoothConnectionStatus.Disconnected:
                Disconnect();
                break;
            case BluetoothConnectionStatus.Connected:
                IsConnected = true;
                break;
        }
    }

    private void Uart_ReceivedData(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        LatestTimestamp = _dateTimeService.Now;
        //Debug.Print(LatestTimestamp.ToString());
        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        var input = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(input);
        var receivedMsg = Encoding.UTF8.GetString(input);
        //Debug.Print(LatestTimestamp + receivedMsg);

        DataReceived?.Invoke(this, receivedMsg);
    }

    public async Task SendAsync(string msg)
    {
        Debug.Print("Sent " + msg);
        await _syncSemaphore.WaitAsync();
        try
        {
            using var writer = new DataWriter();
            writer.WriteString(msg);
            var res = await _txCharacteristic.WriteValueAsync(writer.DetachBuffer(),
                GattWriteOption.WriteWithoutResponse);
            if (res != GattCommunicationStatus.Success)
                throw new VerificationException("Failed to send data to device!");
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }

    public void Disconnect()
    {
        Dispose();
    }

    public void Dispose()
    {
        lock (_disposingSyncRoot)
        {
            if (IsDisposed) return;

            Debug.Print("Disposing BLE device");
            IsConnected = false;
            IsDisposed = true;
            _incomingHeartbeatWatchdog.Stop();
            _incomingHeartbeatWatchdog.Dispose();
            _outgoingHeartbeat.Stop();
            _outgoingHeartbeat.Dispose();
            //https://stackoverflow.com/questions/39599252/windows-ble-uwp-disconnect

            if (_rxCharacteristic != null)
            {
                _rxCharacteristic.ValueChanged -= Uart_ReceivedData;
                _rxCharacteristic.Service.Dispose(); // RX and TX share the same service! so only one dispose is needed
                _rxCharacteristic = null;
                _txCharacteristic = null;
            }

            if (_device != null)
            {
                _device.ConnectionStatusChanged -= BleDeviceOnConnectionStatusChanged;
                _device?.Dispose();
                _device = null;
            }
            Debug.Print("Disposed BLE device");
        }
    }
}