using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

namespace Mebster.Myodam.UI.WPF.ViewModels;

public interface IDeviceCalibrationViewModel
{
    string NoLoadSensorValue { get; set; }
    string NominalLoadSensorValue { get; set; }
    bool IsCalibrationRunning { get; }
    IAsyncRelayCommand CalibrateNoLoadSensorValueCommand { get; }
    IAsyncRelayCommand CalibrateNominalLoadSensorValueCommand { get; }
    AppConfiguration AppConfiguration { get; }
    Task LoadAsync();
}

public class DeviceCalibrationViewModel : ViewModelBase, IDeviceCalibrationViewModel
{
    private readonly IMyodamManager _myodamManager;
    private readonly IMessageDialogService _dialogService;
    private readonly IAppConfigurationLoader _configurationLoader;

    public DeviceCalibration Model { get; private set; } = new(); // must be public prop

    public AppConfiguration AppConfiguration { get; }

    [Required]
    public string NoLoadSensorValue
    {
        get => Model.NoLoadSensorValue.ToString();
        set
        {
            if (!double.TryParse(value, out var parsedValue)) return;
            if (parsedValue < 0) return;

            Model.NoLoadSensorValue = parsedValue;
            _configurationLoader.SaveConfig();
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
            _configurationLoader.SaveConfig();
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
        IMyodamManager myodamManager,
        IAsyncRelayCommandFactory asyncCommandFactory,
        IMessageDialogService dialogService,
        IAppConfigurationLoader configurationLoader)
    {
        _myodamManager = myodamManager;
        _dialogService = dialogService;
        _configurationLoader = configurationLoader;
        _myodamManager.MyodamAvailabilityChanged += MyodamStatusChanged;
        _myodamManager.MeasurementStatusChanged += MyodamStatusChanged;

        AppConfiguration = _configurationLoader.GetConfiguration();
        _myodamManager.SetDeviceAddressFilter(AppConfiguration.MyodamAddress);
        Model = AppConfiguration.MyodamCalibration;
        _myodamManager.Calibration = Model;

        CalibrateNoLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNoLoadSensorValueAsync, CanCalibrateExecute);
        CalibrateNominalLoadSensorValueCommand = asyncCommandFactory.Create(CalibrateNominalLoadSensorValueAsync, CanCalibrateExecute);
    }

    public async Task LoadAsync()
    {
        if (!_configurationLoader.IsConfigurationAvailable())
        {
            await _dialogService.ShowInfoDialogAsync($"Configuration file '{_configurationLoader.ConfigurationFileName}' does not exist. Measurement will be disabled.");
        }
    }

    //public void Unsubscribe() // TODO 
    //{
    //    _myodamManager.MyodamAvailabilityChanged -= MyodamStatusChanged; 
    //    _myodamManager.MeasurementStatusChanged -= MyodamStatusChanged; 
    //}

    private void MyodamStatusChanged(object? sender, System.EventArgs e)
    {
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNoLoadSensorValueAsync()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration with nominal load? This will erase the current value.",
            "Start calibration without load?");
        if (result != MessageDialogResult.OK) return;

        NoLoadSensorValue = (await _myodamManager.MyodamDevice!.GetSensorCalibrationValueAsync()).ToString();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private async Task CalibrateNominalLoadSensorValueAsync()
    {
        var result = await _dialogService.ShowOkCancelDialogAsync(
            "Are you sure you want to perform calibration without load? This will erase the current value.",
            "Start calibration with nominal load?");
        if (result != MessageDialogResult.OK) { return; }

        NominalLoadSensorValue = (await _myodamManager.MyodamDevice!.GetSensorCalibrationValueAsync()).ToString();
        NotifyCalibrationCommandsCanExecuteChanged();
    }

    private void NotifyCalibrationCommandsCanExecuteChanged()
    {
        RunInViewContext(() =>
            {
                CalibrateNoLoadSensorValueCommand.NotifyCanExecuteChanged();
                CalibrateNominalLoadSensorValueCommand.NotifyCanExecuteChanged();
            });
    }

    private bool CanCalibrateExecute()
    {
        return _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected && !_myodamManager.IsCurrentlyMeasuring;
    }
}