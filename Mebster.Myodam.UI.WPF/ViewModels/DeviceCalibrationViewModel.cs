using System.Threading.Tasks;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace Mebster.Myodam.UI.WPF.ViewModels;

public class DeviceCalibrationViewModel: ViewModelBase
{
    private readonly DeviceCalibration _model;
    private readonly IMyodamManager _myodamManager;
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
        IMyodamManager myodamManager, 
        IAsyncRelayCommandFactory asyncCommandFactory, 
        IMessageDialogService dialogService)
    {
        _model = model;
        _myodamManager = myodamManager;
        _dialogService = dialogService;
        _myodamManager.MyodamAvailabilityChanged += MyodamAvailabilityChanged; // TODO unsub

        CalibrateNoLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNoLoadSensorValue, CanCalibrateExecute);
        CalibrateNominalLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNominalLoadSensorValue, CanCalibrateExecute);
    }

    private void MyodamAvailabilityChanged(object? sender, System.EventArgs e)
    {
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNominalLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration without load? This will erase the previous value.",
            "Start calibration without load?");
        if (result != MessageDialogResult.OK) return;

        NominalLoadSensorValue = await _myodamManager.MyodamDevice!.GetSensorCalibrationValue();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNoLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration with nominal load? This will erase the previous value.",
            "Start calibration with nominal load?");
        if (result != MessageDialogResult.OK) return;

        NoLoadSensorValue = await _myodamManager.MyodamDevice!.GetSensorCalibrationValue();
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
               && _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected
               && !_myodamManager.IsCurrentlyMeasuring;
    }
}