using System.Diagnostics;
using System.Security;
using System.Text;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;

namespace BleRecorder.Infrastructure.Bluetooth;

public class BleDeviceHandler : IBleDeviceHandler
{
    private readonly IDateTimeService _dateTimeService;
    public event EventHandler<string>? DataReceived;
    public event EventHandler? DeviceStatusChanged;

    private BluetoothLEDevice? _device;
    private GattCharacteristic? _rxCharacteristic;
    private GattCharacteristic? _txCharacteristic;
    private readonly System.Timers.Timer _heartbeatWatchdog; // thread-safe timer
    private TimeSpan _incomingHearbeatWatchdogInterval = TimeSpan.FromSeconds(0.4);
    private TimeSpan _incomingHearbeatTimeout = TimeSpan.FromSeconds(1.0); 
    private bool _isConnected;
    private readonly object _syncRoot = new();
    //private string _previousMsg;

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

    public BleDeviceHandler(IDateTimeService dateTimeService, 
        string name, ulong address, short signalStrength, DateTimeOffset timestamp)
    {
        _dateTimeService = dateTimeService;
        Name = name;
        Address = address;
        SignalStrength = signalStrength;
        LatestTimestamp = timestamp;
        _heartbeatWatchdog = new System.Timers.Timer(_incomingHearbeatWatchdogInterval.TotalMilliseconds);
        _heartbeatWatchdog.Elapsed += OnWatchdogPeriodElapsed;

        //_previousMsg = ">SC:001_SF:001_SP:050_ST:01_MC:0\n"; 
    }

    public async Task<IBleDeviceHandler> ConnectDeviceAsync()
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

        _heartbeatWatchdog.Start();

        // var isAccessAllowed = await _device.RequestAccessAsync();

        return this;
    }

    private void OnWatchdogPeriodElapsed(object? sender, ElapsedEventArgs e)
    {
        //Debug.Print(_previousMsg);
        //await Send(_previousMsg); // PROBLEM - when device sends Measurement stopped - the message does not reflect it. 
        // this causes restart of measurement

        var ts = _dateTimeService.Now;
        if (ts - LatestTimestamp < _incomingHearbeatTimeout) return;

        Disconnect();
        //throw new ArgumentException("Device has been disconnected!");
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
        //Debug.Print(receivedMsg);

        DataReceived?.Invoke(this, receivedMsg);
    }

    public async Task SendAsync(string msg)
    {
        //_previousMsg = msg;
        using var writer = new DataWriter();
        writer.WriteString(msg);
        var res = await _txCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
        if (res != GattCommunicationStatus.Success) throw new VerificationException("Failed to send data to device!");
    }

    public void Disconnect()
    {
        Dispose();
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (IsDisposed) return;

            Debug.Print("Disposing");
            IsConnected = false;
            IsDisposed = true;
            _heartbeatWatchdog.Stop();
            _heartbeatWatchdog.Dispose();
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
            Debug.Print("Disposed");
        }
    }
}