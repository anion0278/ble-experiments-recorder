using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public MyodamDevice? MyodamDevice { get; }
    MyodamAvailabilityStatus MyodamAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectMyodam();
}

public class MyodamManager : IMyodamManager
{
    public event EventHandler? MyodamAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    private readonly IBluetoothManager _bluetoothManager;
    private MyodamAvailabilityStatus _myodamAvailability;
    private const string _myodamName = "MYODAM";
    public MyodamDevice? MyodamDevice { get; private set; }

    public MyodamAvailabilityStatus MyodamAvailability
    {
        get => _myodamAvailability;
        set
        {
            if (value == _myodamAvailability) return;
            _myodamAvailability = value;
            MyodamAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsCurrentlyMeasuring => MyodamDevice?.IsCurrentlyMeasuring ?? false;

    public MyodamManager(IBluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        _bluetoothManager.AddDeviceNameFilter(_myodamName);
        _bluetoothManager.AvailableBleDevices.CollectionChanged += OnAvailableDevicesChanged;
        MyodamAvailability = MyodamAvailabilityStatus.DisconnectedUnavailable;
        _bluetoothManager.StartScanning();
    }

    private void OnAvailableDevicesChanged(object? sender, EventArgs e)
    {
        if (MyodamDevice != null && MyodamDevice.IsConnected) return;

        MyodamAvailability = _bluetoothManager.AvailableBleDevices.Any(IsMyodamDevice)
            ? MyodamAvailabilityStatus.DisconnectedAvailable
            : MyodamAvailabilityStatus.DisconnectedUnavailable;
    }

    private bool IsMyodamDevice(BleDeviceHandler deviceHandler)
    {
        return deviceHandler.Name.Equals(_myodamName);
    }

    public async Task ConnectMyodam()
    {
        if (MyodamDevice != null)
        {
            MyodamDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
            MyodamDevice.MeasurementStatusChanged -= OnMeasurementStatusChanged;
            MyodamDevice.Disconnect();
            MyodamDevice = null;
        }

        var myodamDevices = _bluetoothManager.AvailableBleDevices.Where(IsMyodamDevice).ToArray();
        if (myodamDevices.Length > 1) throw new Exception("There is more than one myodam device!");
        MyodamDevice = new MyodamDevice(await myodamDevices.Single().ConnectDevice());
        MyodamAvailability = MyodamAvailabilityStatus.Connected;
        MyodamDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
        MyodamDevice.MeasurementStatusChanged += OnMeasurementStatusChanged;
    }

    private void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnConnectionStatusChanged(object? o, EventArgs eventArgs)
    {
        if (MyodamDevice != null && !MyodamDevice.IsConnected)
        {
            MyodamAvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}