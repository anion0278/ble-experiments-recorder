using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Navigation;

public interface INavigationItemViewModelFactory
{
    INavigationItemViewModel GetViewModel(TestSubject ts);
}