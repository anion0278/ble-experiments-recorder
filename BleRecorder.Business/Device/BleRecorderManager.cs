using System.Diagnostics;
using BleRecorder.Business.Exception;
using BleRecorder.Common.Services;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public class BleRecorderManager : IBleRecorderManager
{
    public event EventHandler? BleRecorderAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;
    private readonly IBluetoothManager _bluetoothManager;
    private readonly IBleRecorderReplyParser _messageParser;
    private readonly ISynchronizationContextProvider _synchronizationContextProvider;
    private BleRecorderAvailabilityStatus _bleRecorderAvailability;
    private ulong? _acceptedAddress;
    private const string _bleRecorderName = "Aggregator";
    public IBleRecorderDevice? BleRecorderDevice { get; private set; }

    public BleRecorderAvailabilityStatus BleRecorderAvailability
    {
        get => _bleRecorderAvailability;
        set
        {
            if (value == _bleRecorderAvailability) return;
            _bleRecorderAvailability = value;
            BleRecorderAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public DeviceCalibration Calibration { get; set; }

    public bool IsCurrentlyMeasuring => (BleRecorderDevice?.IsCurrentlyMeasuring ?? false) || (BleRecorderDevice?.IsCalibrating ?? false);

    public BleRecorderManager(
        IBluetoothManager bluetoothManager, 
        IBleRecorderReplyParser messageParser,
        ISynchronizationContextProvider synchronizationContextProvider)
    {
        _bluetoothManager = bluetoothManager;
        _messageParser = messageParser;
        _synchronizationContextProvider = synchronizationContextProvider;
        _bluetoothManager.AddDeviceNameFilter(_bleRecorderName);
        _bluetoothManager.AvailableBleDevices.CollectionChanged += OnAvailableDevicesChanged;
        BleRecorderAvailability = BleRecorderAvailabilityStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void OnAvailableDevicesChanged(object? sender, EventArgs e)
    {
        if (BleRecorderDevice != null && BleRecorderDevice.IsConnected) return;

        BleRecorderAvailability = _bluetoothManager.AvailableBleDevices.Any(IsBleRecorderDevice)
            ? BleRecorderAvailabilityStatus.DisconnectedAvailable
            : BleRecorderAvailabilityStatus.DisconnectedUnavailable;
    }

    public void SetDeviceAddressFilter(ulong? acceptedAddress)
    {
        _acceptedAddress = acceptedAddress;
    }

    private bool IsBleRecorderDevice(BluetoothDeviceHandler deviceHandler)
    {
        bool isBleRecorderDevice = deviceHandler.Name.Equals(_bleRecorderName);
        if (_acceptedAddress is not null) return isBleRecorderDevice && deviceHandler.Address.Equals(_acceptedAddress);
        return isBleRecorderDevice;
    }

    public async Task ConnectBleRecorderAsync()
    {
        if (BleRecorderDevice != null) OnDeviceDisconnection();

        var bleRecorderDevices = _bluetoothManager.AvailableBleDevices.Where(IsBleRecorderDevice).ToArray();

        if (bleRecorderDevices.Length > 1) throw new System.Exception("There is more than one bleRecorder device with provided address!");

        IBluetoothDeviceHandler bleDevice;
        try
        {
            bleDevice = await bleRecorderDevices.Single().ConnectDeviceAsync();
        }
        catch (System.Exception ex)
        {
            throw new DeviceConnectionException(ex);
        }

        BleRecorderDevice = new BleRecorderDeviceUiWrapper(
            new BleRecorderDevice(bleDevice, _messageParser, _synchronizationContextProvider, Calibration),
            _synchronizationContextProvider);
        BleRecorderAvailability = BleRecorderAvailabilityStatus.Connected;
        BleRecorderDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
        BleRecorderDevice.MeasurementStatusChanged += OnMeasurementStatusChanged;
        BleRecorderDevice.BatteryStatusChanged += OnDevicePropertyChanged;
    }

    private void OnDeviceDisconnection()
    {
        BleRecorderDevice!.ConnectionStatusChanged -= OnConnectionStatusChanged;
        BleRecorderDevice.MeasurementStatusChanged -= OnMeasurementStatusChanged;
        BleRecorderDevice.BatteryStatusChanged -= OnDevicePropertyChanged;
        BleRecorderDevice = null;
        BleRecorderAvailability = BleRecorderAvailabilityStatus.DisconnectedUnavailable;
    }

    private void OnDevicePropertyChanged(object? sender, EventArgs e)
    {
        DevicePropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnConnectionStatusChanged(object? o, EventArgs eventArgs)
    {
        if (BleRecorderDevice != null && !BleRecorderDevice.IsConnected)
        {
            OnDeviceDisconnection();
        }
    }
}