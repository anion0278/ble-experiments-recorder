using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Navigation;

public class NavigationItemViewModelFactory : INavigationItemViewModelFactory
{
    private readonly IMessenger _messenger;

    public NavigationItemViewModelFactory(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public INavigationItemViewModel GetViewModel(TestSubject ts)
    {
        return new NavigationItemViewModel(ts!, _messenger);
    }
}