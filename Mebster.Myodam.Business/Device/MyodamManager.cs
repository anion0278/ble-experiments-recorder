using System.Diagnostics;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamAvailabilityChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? DevicePropertyChanged;
    MyodamDevice? MyodamDevice { get; }
    DeviceCalibration Calibration { get; set; }
    StimulationParameters CurrentStimulationParameters { get; set; }
    MyodamAvailabilityStatus MyodamAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectMyodam();
}

public class MyodamManager : IMyodamManager
{
    public event EventHandler? MyodamAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;
    private readonly IBluetoothManager _bluetoothManager;
    private readonly IMyodamMessageParser _messageParser;
    private MyodamAvailabilityStatus _myodamAvailability;
    private const string _myodamName = "MYODAM-TEST";
    public MyodamDevice? MyodamDevice { get; private set; }

    public StimulationParameters CurrentStimulationParameters { get; set; }

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

    public DeviceCalibration Calibration { get; set; }

    public bool IsCurrentlyMeasuring => (MyodamDevice?.IsCurrentlyMeasuring ?? false) || (MyodamDevice?.IsCalibrating ?? false);

    public MyodamManager(IBluetoothManager bluetoothManager, IMyodamMessageParser messageParser)
    {
        _bluetoothManager = bluetoothManager;
        _messageParser = messageParser;
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

    private static bool IsMyodamDevice(BleDeviceHandler deviceHandler)
    {
        return deviceHandler.Name.Equals(_myodamName);
    }

    public async Task ConnectMyodam()
    {
        if (MyodamDevice != null) OnDeviceDisconnection();

        var myodamDevices = _bluetoothManager.AvailableBleDevices.Where(IsMyodamDevice).ToArray();

        // TODO Handle multiple devices in a single room
        if (myodamDevices.Length > 1) throw new System.Exception("There is more than one myodam device!");

        IBleDeviceHandler bleDevice;
        try
        {
            bleDevice = await myodamDevices.Single().ConnectDevice();
        }
        catch (System.Exception ex)
        {
            throw new DeviceConnectionException(ex);
        }

        MyodamDevice = new MyodamDevice(this, bleDevice, _messageParser);
        MyodamAvailability = MyodamAvailabilityStatus.Connected;
        MyodamDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
        MyodamDevice.MeasurementStatusChanged += OnMeasurementStatusChanged;
        MyodamDevice.BatteryStatusChanged += OnDevicePropertyChanged;
    }

    private void OnDeviceDisconnection()
    {
        MyodamDevice!.ConnectionStatusChanged -= OnConnectionStatusChanged;
        MyodamDevice.MeasurementStatusChanged -= OnMeasurementStatusChanged;
        MyodamDevice.BatteryStatusChanged -= OnDevicePropertyChanged;
        MyodamDevice = null;
        MyodamAvailability = MyodamAvailabilityStatus.DisconnectedUnavailable;
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
        if (MyodamDevice != null && !MyodamDevice.IsConnected)
        {
            OnDeviceDisconnection();
        }
    }
}