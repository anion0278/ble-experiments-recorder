using System.Diagnostics;
using BleRecorder.Business.Exception;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public interface IBleRecorderManager
{
    event EventHandler BleRecorderAvailabilityChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? DevicePropertyChanged;
    BleRecorderDevice? BleRecorderDevice { get; }
    DeviceCalibration Calibration { get; set; }
    StimulationParameters CurrentStimulationParameters { get; set; }
    BleRecorderAvailabilityStatus BleRecorderAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectBleRecorder();
}

public class BleRecorderManager : IBleRecorderManager
{
    public event EventHandler? BleRecorderAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;
    private readonly IBluetoothManager _bluetoothManager;
    private readonly IBleRecorderMessageParser _messageParser;
    private BleRecorderAvailabilityStatus _bleRecorderAvailability;
    private const string _bleRecorderName = "Aggregator-TEST";
    public BleRecorderDevice? BleRecorderDevice { get; private set; }

    public StimulationParameters CurrentStimulationParameters { get; set; }

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

    public BleRecorderManager(IBluetoothManager bluetoothManager, IBleRecorderMessageParser messageParser)
    {
        _bluetoothManager = bluetoothManager;
        _messageParser = messageParser;
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

    private static bool IsBleRecorderDevice(BleDeviceHandler deviceHandler)
    {
        return deviceHandler.Name.Equals(_bleRecorderName);
    }

    public async Task ConnectBleRecorder()
    {
        if (BleRecorderDevice != null) OnDeviceDisconnection();

        var bleRecorderDevices = _bluetoothManager.AvailableBleDevices.Where(IsBleRecorderDevice).ToArray();

        // TODO Handle multiple devices in a single room
        if (bleRecorderDevices.Length > 1) throw new System.Exception("There is more than one bleRecorder device!");

        IBleDeviceHandler bleDevice;
        try
        {
            bleDevice = await bleRecorderDevices.Single().ConnectDevice();
        }
        catch (System.Exception ex)
        {
            throw new DeviceConnectionException(ex);
        }

        BleRecorderDevice = new BleRecorderDevice(this, bleDevice, _messageParser);
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