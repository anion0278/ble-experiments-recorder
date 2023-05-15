using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Navigation;

public interface INavigationItemViewModelFactory
{
    INavigationTestSubjectItemViewModel GetViewModel(TestSubject ts);
}