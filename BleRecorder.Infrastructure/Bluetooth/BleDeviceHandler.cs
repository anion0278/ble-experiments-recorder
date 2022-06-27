using System.Diagnostics;
using System.Security;
using System.Text;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using BleRecorder.Models.Device;

namespace BleRecorder.Infrastructure.Bluetooth;

public class BleDeviceHandler : IBleDeviceHandler
{
    public event EventHandler<string>? DataReceived;
    public event EventHandler? DeviceStatusChanged;

    private BluetoothLEDevice? _device;
    private GattCharacteristic? _rxCharacteristic;
    private GattCharacteristic? _txCharacteristic;
    private readonly System.Timers.Timer _heartbeatWatchdog; // thread-safe timer
    private TimeSpan _hearbeatWatchdogInterval = TimeSpan.FromSeconds(0.3);
    private TimeSpan _hearbeatTimeout = TimeSpan.FromSeconds(0.8);
    private bool _isConnected;

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

    public BleDeviceHandler(string name, ulong address, short signalStrength, DateTimeOffset timestamp)
    {
        Name = name;
        Address = address;
        SignalStrength = signalStrength;
        LatestTimestamp = timestamp;
        _heartbeatWatchdog = new System.Timers.Timer(_hearbeatWatchdogInterval.TotalMilliseconds);
        _heartbeatWatchdog.Elapsed += OnWatchdogPeriodElapsed;
    }

    public async Task<IBleDeviceHandler> ConnectDevice()
    {
        if (IsDisposed) throw new ObjectDisposedException("BLE devise was disposed! Create a new object.");

        _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
        _device.ConnectionStatusChanged += BleDeviceOnConnectionStatusChanged;

        //var y = bleDevice.DeviceInformation.Pairing.CanPair;
        //var x = bleDevice.ConnectionStatus;
        //var allDeviceServices = (await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached)).Services.ToArray();

        var uartGuid = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        var uartService = (await _device.GetGattServicesForUuidAsync(uartGuid)).Services.Single();

        if ((await _device.GetGattServicesForUuidAsync(uartGuid)).Status != GattCommunicationStatus.Success) throw new Exception("Connection with BLE device failed!");

        var rxId = new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        var txId = new Guid("6e400002-b5a3-f393-e0a9-e50e24dcca9e");

        var uartCharacteristics = (await uartService.GetCharacteristicsAsync()).Characteristics;
        _rxCharacteristic = uartCharacteristics.Single(s => s.Uuid == rxId);
        _txCharacteristic = uartCharacteristics.Single(s => s.Uuid == txId);

        await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify); // required to receive data
        _rxCharacteristic.ValueChanged += Uart_ReceivedData;

        _heartbeatWatchdog.Start();

        return this;
    }

    private void OnWatchdogPeriodElapsed(object? sender, ElapsedEventArgs e)
    {
        var ts = DateTimeOffset.Now;
        if (ts - LatestTimestamp < _hearbeatTimeout) return;

        Dispose();
    }

    private void BleDeviceOnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        switch (sender.ConnectionStatus) // we cannot rely on this prop, since its not updated when the device is disconnected "externally"
        {
            case BluetoothConnectionStatus.Disconnected:
                Dispose();
                break;
            case BluetoothConnectionStatus.Connected:
                IsConnected = true;
                break;
        }
    }

    private void Uart_ReceivedData(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        LatestTimestamp = DateTimeOffset.Now;
        Debug.Print(LatestTimestamp.ToString());
        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        var input = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(input);
        var receivedMsg = Encoding.UTF8.GetString(input);

        DataReceived?.Invoke(this, receivedMsg);
    }

    public async Task Send(string msg)
    {
        using var writer = new DataWriter();
        writer.WriteString("Some long string command");
        var res = await _txCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
        if (res != GattCommunicationStatus.Success) throw new VerificationException("Failed to send data to device!");
    }

    public void Disconnect()
    {
        Dispose();
    }

    public void Dispose()
    {
        IsConnected = false;
        IsDisposed = true;
        _heartbeatWatchdog.Stop();
        _heartbeatWatchdog.Dispose();
        //https://stackoverflow.com/questions/39599252/windows-ble-uwp-disconnect

        if (_rxCharacteristic != null)
        {
            _rxCharacteristic.ValueChanged -= Uart_ReceivedData;
            _rxCharacteristic.Service.Dispose();
            _rxCharacteristic = null;
        }

        if (_txCharacteristic != null)
        {
            _txCharacteristic.Service.Dispose();
            _txCharacteristic = null;
        }

        if (_device != null)
        {
            _device.ConnectionStatusChanged -= BleDeviceOnConnectionStatusChanged;
            _device?.Dispose();
            _device = null;
        }
    }
}