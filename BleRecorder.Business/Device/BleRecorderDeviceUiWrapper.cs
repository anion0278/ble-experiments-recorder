using BleRecorder.Common.Services;
using BleRecorder.Models;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderDeviceUiWrapper : IBleRecorderDevice
{
    private readonly IBleRecorderDevice _bleRecorderDevice;
    private readonly ISynchronizationContextProvider _contextProvider;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? ErrorChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;
    public BleRecorderError Error => _bleRecorderDevice.Error;

    public TimeSpan DataRequestInterval => _bleRecorderDevice.DataRequestInterval;

    public Percentage StimulatorBattery => _bleRecorderDevice.StimulatorBattery;

    public Percentage ControllerBattery => _bleRecorderDevice.ControllerBattery;

    public bool IsConnected => _bleRecorderDevice.IsConnected;

    public bool IsCalibrating => _bleRecorderDevice.IsCalibrating;

    public bool IsCurrentlyMeasuring => _bleRecorderDevice.IsCurrentlyMeasuring;

    public DeviceCalibration Calibration
    {
        get => _bleRecorderDevice.Calibration;
        set => _bleRecorderDevice.Calibration = value;
    }

    public StimulationParameters CurrentParameters
    {
        get => _bleRecorderDevice.CurrentParameters;
        set => _bleRecorderDevice.CurrentParameters = value;
    }

    public BleRecorderDeviceUiWrapper(IBleRecorderDevice bleRecorderDevice, ISynchronizationContextProvider contextProvider)
    {
        _bleRecorderDevice = bleRecorderDevice;
        _contextProvider = contextProvider;

        _bleRecorderDevice.NewValueReceived += _bleRecorderDevice_NewValueReceived;
        _bleRecorderDevice.ConnectionStatusChanged += _bleRecorderDevice_ConnectionStatusChanged; 
        _bleRecorderDevice.ErrorChanged += _bleRecorderDevice_ErrorChanged;
        _bleRecorderDevice.MeasurementStatusChanged += _bleRecorderDevice_MeasurementStatusChanged;
        _bleRecorderDevice.BatteryStatusChanged += _bleRecorderDevice_BatteryStatusChanged;
    }

    private void _bleRecorderDevice_BatteryStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => BatteryStatusChanged?.Invoke(sender, e));
    }

    private void _bleRecorderDevice_MeasurementStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => MeasurementStatusChanged?.Invoke(sender, e));
    }

    private void _bleRecorderDevice_ErrorChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => ErrorChanged?.Invoke(sender, e));
    }

    private void _bleRecorderDevice_ConnectionStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => ConnectionStatusChanged?.Invoke(sender, e));
    }

    private void _bleRecorderDevice_NewValueReceived(object? sender, MeasuredValue e)
    {
        _contextProvider.RunInContext(() => NewValueReceived?.Invoke(sender, e));
    }

    public Task SendMsgAsync(BleRecorderRequestMessage message)
    {
        return _bleRecorderDevice.SendMsgAsync(message);
    }

    public Task<double> GetSensorCalibrationValueAsync()
    {
        return _bleRecorderDevice.GetSensorCalibrationValueAsync();
    }

    public Task StartMeasurementAsync(StimulationParameters parameters, MeasurementType measurementType)
    {
        return _bleRecorderDevice.StartMeasurementAsync(parameters, measurementType);
    }

    public Task StopMeasurementAsync()
    {
        return _bleRecorderDevice.StopMeasurementAsync();
    }

    public Task DisableFesAndDisconnectAsync()
    {
        return _bleRecorderDevice.DisableFesAndDisconnectAsync();
    }

    public async Task DisconnectAsync()
    {
        await _bleRecorderDevice.DisconnectAsync();
        _bleRecorderDevice.NewValueReceived -= _bleRecorderDevice_NewValueReceived;
        _bleRecorderDevice.ConnectionStatusChanged -= _bleRecorderDevice_ConnectionStatusChanged;
        _bleRecorderDevice.ErrorChanged -= _bleRecorderDevice_ErrorChanged;
        _bleRecorderDevice.MeasurementStatusChanged -= _bleRecorderDevice_MeasurementStatusChanged;
        _bleRecorderDevice.BatteryStatusChanged -= _bleRecorderDevice_BatteryStatusChanged;
    }
}