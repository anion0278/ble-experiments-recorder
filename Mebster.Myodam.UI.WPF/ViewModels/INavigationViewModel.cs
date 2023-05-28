using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public interface INavigationViewModel
    {
        Task LoadAsync();
        public ICollectionView TestSubjectsNavigationItems { get; }
        public int ControllerBatteryPercentage { get; }
        public MyodamError DeviceError { get; }
        public string FullNameFilter { get; set; }
        public int StimulatorBatteryPercentage { get; }
        public ICommand OpenDetailViewCommand { get; }
        public ICommand SelectAllFilteredCommand { get; }
        public ICommand DeselectAllFilteredCommand { get; }
        public IAsyncRelayCommand ChangeMyodamConnectionCommand { get; }
        public IAsyncRelayCommand ExportSelectedCommand { get; }
        public MyodamAvailabilityStatus MyodamAvailability { get; }
        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }
    }
}