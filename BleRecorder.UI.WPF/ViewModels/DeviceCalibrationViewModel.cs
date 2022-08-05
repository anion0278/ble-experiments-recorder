using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleRecorder.UI.WPF.ViewModels;

public interface IDeviceCalibrationViewModel
{
    string NoLoadSensorValue { get; set; }
    string NominalLoadSensorValue { get; set; }
    bool IsCalibrationRunning { get; }
    IAsyncRelayCommand CalibrateNoLoadSensorValueCommand { get; }
    IAsyncRelayCommand CalibrateNominalLoadSensorValueCommand { get; }

    Task LoadAsync();
}

public class DeviceCalibrationViewModel : ViewModelBase, IDeviceCalibrationViewModel
{
    private readonly IDeviceCalibrationRepository _deviceCalibrationRepository;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly IMessageDialogService _dialogService;

    public DeviceCalibration Model { get; private set; } = new(); // must be public prop

    [Required]
    public string NoLoadSensorValue
    {
        get => Model.NoLoadSensorValue.ToString();
        set
        {
            if (!double.TryParse(value, out var parsedValue)) return;
            if (parsedValue < 0) return;

            Model.NoLoadSensorValue = parsedValue;
            _deviceCalibrationRepository.Save();
        }
    }

    [Required]
    public string NominalLoadSensorValue
    {
        get => Model.NominalLoadSensorValue.ToString();
        set
        {
            if (!double.TryParse(value, out var parsedValue)) return;
            if (parsedValue < 0) return;

            Model.NominalLoadSensorValue = parsedValue;
            _deviceCalibrationRepository.Save();
        }
    }

    public bool IsCalibrationRunning => CalibrateNoLoadSensorValueCommand.IsRunning || CalibrateNominalLoadSensorValueCommand.IsRunning;

    public IAsyncRelayCommand CalibrateNoLoadSensorValueCommand { get; }
    public IAsyncRelayCommand CalibrateNominalLoadSensorValueCommand { get; }

    /// <summary>
    /// Design-time ctor
    /// </summary>
    public DeviceCalibrationViewModel() { }

    public DeviceCalibrationViewModel(
        IDeviceCalibrationRepository deviceCalibrationRepository,
        IBleRecorderManager bleRecorderManager,
        IAsyncRelayCommandFactory asyncCommandFactory,
        IMessageDialogService dialogService)
    {
        _deviceCalibrationRepository = deviceCalibrationRepository;
        _bleRecorderManager = bleRecorderManager;
        _dialogService = dialogService;
        _bleRecorderManager.BleRecorderAvailabilityChanged += BleRecorderAvailabilityChanged; // TODO unsub

        CalibrateNoLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNoLoadSensorValue, CanCalibrateExecute);
        CalibrateNominalLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNominalLoadSensorValue, CanCalibrateExecute);
    }

    public async Task LoadAsync()
    {
        Model = (await _deviceCalibrationRepository.GetByIdAsync(1))!;
    }

    private void BleRecorderAvailabilityChanged(object? sender, System.EventArgs e)
    {
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNoLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration with nominal load? This will erase the current value.",
            "Start calibration with nominal load?");
        if (result != MessageDialogResult.OK) return;

        NoLoadSensorValue = (await _bleRecorderManager.BleRecorderDevice!.GetSensorCalibrationValue()).ToString();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNominalLoadSensorValue()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration without load? This will erase the current value.",
            "Start calibration without load?");
        if (result != MessageDialogResult.OK) { return; }

        NominalLoadSensorValue = (await _bleRecorderManager.BleRecorderDevice!.GetSensorCalibrationValue()).ToString();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private void NotifyCalibrationCommandsCanExecuteChanged()
    {
        ViewSynchronizationContext.Send(_ => 
            {
                CalibrateNoLoadSensorValueCommand.NotifyCanExecuteChanged();
                CalibrateNominalLoadSensorValueCommand.NotifyCanExecuteChanged();
            }, null);
    }

    private bool CanCalibrateExecute()
    {
        return _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected && !_bleRecorderManager.IsCurrentlyMeasuring;
    }
}