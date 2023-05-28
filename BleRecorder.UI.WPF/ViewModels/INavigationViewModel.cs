using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;

namespace BleRecorder.UI.WPF.ViewModels
{
    public interface INavigationViewModel
    {
        Task LoadAsync();
        public ICollectionView TestSubjectsNavigationItems { get; }
        public int ControllerBatteryPercentage { get; }
        public BleRecorderError DeviceError { get; }
        public string FullNameFilter { get; set; }
        public int StimulatorBatteryPercentage { get; }
        public ICommand OpenDetailViewCommand { get; }
        public ICommand SelectAllFilteredCommand { get; }
        public ICommand DeselectAllFilteredCommand { get; }
        public IAsyncRelayCommand ChangeBleRecorderConnectionCommand { get; }
        public IAsyncRelayCommand ExportSelectedCommand { get; }
        public BleRecorderAvailabilityStatus BleRecorderAvailability { get; }
        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }
    }
}