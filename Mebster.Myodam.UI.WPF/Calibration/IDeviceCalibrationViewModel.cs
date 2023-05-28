using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

namespace Mebster.Myodam.UI.WPF.Calibration;

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