using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamStatusChanged;
    public MyodamDevice? MyodamDevice { get; }
    MyodamAvailabilityStatus MyodamAvailability { get; set; }
    Task ConnectMyodam();
}

public class MyodamManager : IMyodamManager
{
    private readonly IBluetoothManager _bluetoothManager;
    private MyodamAvailabilityStatus _myodamAvailability;
    public MyodamDevice? MyodamDevice { get; private set; }
    public event EventHandler? MyodamStatusChanged;

    private string _myodamName = "MYODAM";

    public MyodamAvailabilityStatus MyodamAvailability
    {
        get => _myodamAvailability;
        set
        {
            _myodamAvailability = value;
            MyodamStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public MyodamManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        _bluetoothManager.AvailableBleDevices.CollectionChanged += OnAvailableDevicesChanged;
        MyodamAvailability = MyodamAvailabilityStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void OnAvailableDevicesChanged(object? sender, EventArgs e)
    {
        MyodamAvailability = _bluetoothManager.AvailableBleDevices.Any(IsMyodamDevice)
            ? MyodamAvailabilityStatus.DisconnectedAvailable
            : MyodamAvailabilityStatus.DisconnectedUnavailable;
    }

    private bool IsMyodamDevice(BleAvailableDevice device)
    {
        return device.Name.Equals(_myodamName);
    }

    public async Task ConnectMyodam()
    {
        MyodamDevice?.Disconnect();

        var myodamDevices = _bluetoothManager.AvailableBleDevices.Where(IsMyodamDevice).ToArray();
        if (myodamDevices.Length > 1) throw new Exception("There is more than one myodam device!");
        MyodamDevice = new MyodamDevice(await myodamDevices.Single().ConnectDevice());
        MyodamAvailability = MyodamAvailabilityStatus.Connected;
    }

}