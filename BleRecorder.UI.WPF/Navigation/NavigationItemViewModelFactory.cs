using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Navigation;

public class NavigationItemViewModelFactory : INavigationItemViewModelFactory
{
    private readonly IMessenger _messenger;

    public NavigationItemViewModelFactory(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public INavigationTestSubjectItemViewModel GetViewModel(TestSubject ts)
    {
        return new NavigationTestSubjectItemViewModel(ts!, _messenger);
    }
}