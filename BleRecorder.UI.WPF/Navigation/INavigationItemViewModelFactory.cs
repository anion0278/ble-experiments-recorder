using BleRecorder.Models.TestSubjects;

namespace BleRecorder.UI.WPF.Navigation;

public interface INavigationItemViewModelFactory
{
    INavigationItemViewModel GetViewModel(TestSubject ts);
}