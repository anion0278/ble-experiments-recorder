using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class MyodamDeviceUiWrapper : IMyodamDevice
{
    private readonly IMyodamDevice _myodamDevice;
    private readonly ISynchronizationContextProvider _contextProvider;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? ErrorChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;
    public MyodamError Error => _myodamDevice.Error;

    public TimeSpan DataRequestInterval => _myodamDevice.DataRequestInterval;

    public Percentage StimulatorBattery => _myodamDevice.StimulatorBattery;

    public Percentage ControllerBattery => _myodamDevice.ControllerBattery;

    public bool IsConnected => _myodamDevice.IsConnected;

    public bool IsCalibrating => _myodamDevice.IsCalibrating;

    public bool IsCurrentlyMeasuring => _myodamDevice.IsCurrentlyMeasuring;

    public DeviceCalibration Calibration
    {
        get => _myodamDevice.Calibration;
        set => _myodamDevice.Calibration = value;
    }

    public StimulationParameters CurrentParameters
    {
        get => _myodamDevice.CurrentParameters;
        set => _myodamDevice.CurrentParameters = value;
    }

    public MyodamDeviceUiWrapper(IMyodamDevice myodamDevice, ISynchronizationContextProvider contextProvider)
    {
        _myodamDevice = myodamDevice;
        _contextProvider = contextProvider;

        _myodamDevice.NewValueReceived += _myodamDevice_NewValueReceived;
        _myodamDevice.ConnectionStatusChanged += _myodamDevice_ConnectionStatusChanged; 
        _myodamDevice.ErrorChanged += _myodamDevice_ErrorChanged;
        _myodamDevice.MeasurementStatusChanged += _myodamDevice_MeasurementStatusChanged;
        _myodamDevice.BatteryStatusChanged += _myodamDevice_BatteryStatusChanged;
    }

    private void _myodamDevice_BatteryStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => BatteryStatusChanged?.Invoke(sender, e));
    }

    private void _myodamDevice_MeasurementStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => MeasurementStatusChanged?.Invoke(sender, e));
    }

    private void _myodamDevice_ErrorChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => ErrorChanged?.Invoke(sender, e));
    }

    private void _myodamDevice_ConnectionStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => ConnectionStatusChanged?.Invoke(sender, e));
    }

    private void _myodamDevice_NewValueReceived(object? sender, MeasuredValue e)
    {
        _contextProvider.RunInContext(() => NewValueReceived?.Invoke(sender, e));
    }

    public Task SendMsgAsync(MyodamRequestMessage message)
    {
        return _myodamDevice.SendMsgAsync(message);
    }

    public Task<double> GetSensorCalibrationValueAsync()
    {
        return _myodamDevice.GetSensorCalibrationValueAsync();
    }

    public Task StartMeasurementAsync(StimulationParameters parameters, MeasurementType measurementType)
    {
        return _myodamDevice.StartMeasurementAsync(parameters, measurementType);
    }

    public Task StopMeasurementAsync()
    {
        return _myodamDevice.StopMeasurementAsync();
    }

    public Task DisableFesAndDisconnectAsync()
    {
        return _myodamDevice.DisableFesAndDisconnectAsync();
    }

    public async Task DisconnectAsync()
    {
        await _myodamDevice.DisconnectAsync();
        _myodamDevice.NewValueReceived -= _myodamDevice_NewValueReceived;
        _myodamDevice.ConnectionStatusChanged -= _myodamDevice_ConnectionStatusChanged;
        _myodamDevice.ErrorChanged -= _myodamDevice_ErrorChanged;
        _myodamDevice.MeasurementStatusChanged -= _myodamDevice_MeasurementStatusChanged;
        _myodamDevice.BatteryStatusChanged -= _myodamDevice_BatteryStatusChanged;
    }
}