using System.Threading.Tasks;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleRecorder.UI.WPF.ViewModels;

public class DeviceCalibrationViewModel: ViewModelBase
{
    private readonly DeviceCalibration _model;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly IMessageDialogService _dialogService;

    public double NoLoadSensorValue
    {
        get => _model.NoLoadSensorValue;
        set => _model.NoLoadSensorValue = value;
    }

    public double NominalLoadSensorValue
    {
        get => _model.NominalLoadSensorValue;
        set => _model.NominalLoadSensorValue = value;
    }

    public bool IsCalibrationRunning => CalibrateNoLoadSensorValueCommand.IsRunning || CalibrateNominalLoadSensorValueCommand.IsRunning;

    public IAsyncRelayCommand CalibrateNoLoadSensorValueCommand { get; }
    public IAsyncRelayCommand CalibrateNominalLoadSensorValueCommand { get; }

    /// <summary>
    /// Design-time ctor
    /// </summary>
    public DeviceCalibrationViewModel() { }

    public DeviceCalibrationViewModel(
        DeviceCalibration model, 
        IBleRecorderManager bleRecorderManager, 
        IAsyncRelayCommandFactory asyncCommandFactory, 
        IMessageDialogService dialogService)
    {
        _model = model;
        _bleRecorderManager = bleRecorderManager;
        _dialogService = dialogService;
        _bleRecorderManager.BleRecorderAvailabilityChanged += BleRecorderAvailabilityChanged; // TODO unsub

        CalibrateNoLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNoLoadSensorValue, CanCalibrateExecute);
        CalibrateNominalLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNominalLoadSensorValue, CanCalibrateExecute);
    }

    private void BleRecorderAvailabilityChanged(object? sender, System.EventArgs e)
    {
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNominalLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration without load? This will erase the previous value.",
            "Start calibration without load?");
        if (result != MessageDialogResult.OK) return;

        NominalLoadSensorValue = await _bleRecorderManager.BleRecorderDevice!.GetSensorCalibrationValue();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNoLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration with nominal load? This will erase the previous value.",
            "Start calibration with nominal load?");
        if (result != MessageDialogResult.OK) return;

        NoLoadSensorValue = await _bleRecorderManager.BleRecorderDevice!.GetSensorCalibrationValue();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private void NotifyCalibrationCommandsCanExecuteChanged()
    {
        CalibrateNoLoadSensorValueCommand.NotifyCanExecuteChanged();
        CalibrateNominalLoadSensorValueCommand.NotifyCanExecuteChanged();
    }

    private bool CanCalibrateExecute()
    {
        return !CalibrateNoLoadSensorValueCommand.IsRunning 
               && !CalibrateNominalLoadSensorValueCommand.IsRunning 
               && _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected
               && !_bleRecorderManager.IsCurrentlyMeasuring;
    }
}