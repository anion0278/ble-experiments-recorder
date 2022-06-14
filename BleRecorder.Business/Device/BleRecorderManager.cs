using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public interface IBleRecorderManager
{
    event EventHandler BleRecorderStatusChanged;
    public BleRecorderDevice BleRecorderDevice { get; }
    DeviceStatus BleRecorderStatus { get; set; }
    Task ConnectBleRecorder();
}

public class BleRecorderManager : IBleRecorderManager
{
    private readonly IBluetoothManager _bluetoothManager;
    private DeviceStatus _bleRecorderStatus;
    public BleRecorderDevice BleRecorderDevice { get; private set; }
    public event EventHandler? BleRecorderStatusChanged;

    public DeviceStatus BleRecorderStatus
    {
        get => _bleRecorderStatus;
        set
        {
            _bleRecorderStatus = value; 
            BleRecorderStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BleRecorderManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        _bluetoothManager.AvailableDevicesChanged += _bluetoothManager_AvailableDevicesChanged;
        BleRecorderStatus = DeviceStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void _bluetoothManager_AvailableDevicesChanged(object? sender, EventArgs e)
    {
        BleRecorderStatus = DeviceStatus.DisconnectedAvailable;
    }

    public async Task ConnectBleRecorder()
    {
        BleRecorderDevice = new BleRecorderDevice(await _bluetoothManager.Connect());
        BleRecorderStatus = DeviceStatus.Connected;
    }

}