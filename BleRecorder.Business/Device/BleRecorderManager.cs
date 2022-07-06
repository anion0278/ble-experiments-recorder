using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public interface IBleRecorderManager
{
    event EventHandler BleRecorderAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public BleRecorderDevice? BleRecorderDevice { get; }
    BleRecorderAvailabilityStatus BleRecorderAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectBleRecorder();
}

public class BleRecorderManager : IBleRecorderManager
{
    public event EventHandler? BleRecorderAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    private readonly IBluetoothManager _bluetoothManager;
    private BleRecorderAvailabilityStatus _bleRecorderAvailability;
    private const string _bleRecorderName = "Aggregator";
    public BleRecorderDevice? BleRecorderDevice { get; private set; }

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

    public bool IsCurrentlyMeasuring => BleRecorderDevice?.IsCurrentlyMeasuring ?? false;

    public BleRecorderManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
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

    private bool IsBleRecorderDevice(BleDeviceHandler deviceHandler)
    {
        return deviceHandler.Name.Equals(_bleRecorderName);
    }

    public async Task ConnectBleRecorder()
    {
        if (BleRecorderDevice != null)
        {
            BleRecorderDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
            BleRecorderDevice.MeasurementStatusChanged -= OnMeasurementStatusChanged;
            BleRecorderDevice.Disconnect();
            BleRecorderDevice = null;
        }

        var bleRecorderDevices = _bluetoothManager.AvailableBleDevices.Where(IsBleRecorderDevice).ToArray();
        if (bleRecorderDevices.Length > 1) throw new Exception("There is more than one bleRecorder device!");
        BleRecorderDevice = new BleRecorderDevice(await bleRecorderDevices.Single().ConnectDevice());
        BleRecorderAvailability = BleRecorderAvailabilityStatus.Connected;
        BleRecorderDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
        BleRecorderDevice.MeasurementStatusChanged += OnMeasurementStatusChanged;
    }

    private void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnConnectionStatusChanged(object? o, EventArgs eventArgs)
    {
        if (BleRecorderDevice != null && !BleRecorderDevice.IsConnected)
        {
            BleRecorderAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}