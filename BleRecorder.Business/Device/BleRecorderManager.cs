using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public interface IBleRecorderManager
{
    event EventHandler BleRecorderStatusChanged;
    public BleRecorderDevice? BleRecorderDevice { get; }
    BleRecorderAvailabilityStatus BleRecorderAvailability { get; set; }
    Task ConnectBleRecorder();
}

public class BleRecorderManager : IBleRecorderManager
{
    private readonly IBluetoothManager _bluetoothManager;
    private BleRecorderAvailabilityStatus _bleRecorderAvailability;
    public BleRecorderDevice? BleRecorderDevice { get; private set; }
    public event EventHandler? BleRecorderStatusChanged;

    private string _bleRecorderName = "Aggregator";

    public BleRecorderAvailabilityStatus BleRecorderAvailability
    {
        get => _bleRecorderAvailability;
        set
        {
            _bleRecorderAvailability = value;
            BleRecorderStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BleRecorderManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        _bluetoothManager.AvailableBleDevices.CollectionChanged += OnAvailableDevicesChanged;
        BleRecorderAvailability = BleRecorderAvailabilityStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void OnAvailableDevicesChanged(object? sender, EventArgs e)
    {
        BleRecorderAvailability = _bluetoothManager.AvailableBleDevices.Any(IsBleRecorderDevice)
            ? BleRecorderAvailabilityStatus.DisconnectedAvailable
            : BleRecorderAvailabilityStatus.DisconnectedUnavailable;
    }

    private bool IsBleRecorderDevice(BleAvailableDevice device)
    {
        return device.Name.Equals(_bleRecorderName);
    }

    public async Task ConnectBleRecorder()
    {
        BleRecorderDevice?.Disconnect();

        var bleRecorderDevices = _bluetoothManager.AvailableBleDevices.Where(IsBleRecorderDevice).ToArray();
        if (bleRecorderDevices.Length > 1) throw new Exception("There is more than one bleRecorder device!");
        BleRecorderDevice = new BleRecorderDevice(await bleRecorderDevices.Single().ConnectDevice());
        BleRecorderAvailability = BleRecorderAvailabilityStatus.Connected;
    }

}