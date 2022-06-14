using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamStatusChanged;
    public MyodamDevice MyodamDevice { get; }
    DeviceStatus MyodamStatus { get; set; }
    Task ConnectMyodam();
}

public class MyodamManager : IMyodamManager
{
    private readonly IBluetoothManager _bluetoothManager;
    private DeviceStatus _myodamStatus;
    public MyodamDevice MyodamDevice { get; private set; }
    public event EventHandler? MyodamStatusChanged;

    public DeviceStatus MyodamStatus
    {
        get => _myodamStatus;
        set
        {
            _myodamStatus = value; 
            MyodamStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public MyodamManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        _bluetoothManager.AvailableDevicesChanged += _bluetoothManager_AvailableDevicesChanged;
        MyodamStatus = DeviceStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void _bluetoothManager_AvailableDevicesChanged(object? sender, EventArgs e)
    {
        MyodamStatus = DeviceStatus.DisconnectedAvailable;
    }

    public async Task ConnectMyodam()
    {
        MyodamDevice = new MyodamDevice(await _bluetoothManager.Connect());
        MyodamStatus = DeviceStatus.Connected;
    }

}