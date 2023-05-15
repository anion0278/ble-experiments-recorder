using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

public class NavigationViewModelCommandsFactory : INavigationViewModelCommandsFactory
{
    private readonly IMessenger _messenger;

    public NavigationViewModelCommandsFactory(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public IRelayCommand GetSelectAllFilteredCommand(INavigationViewModel viewModel)
    {
        return new SelectAllFilteredCommand(viewModel);
    }

    public IRelayCommand GetDeselectAllFilteredCommand(INavigationViewModel viewModel)
    {
        return new DeselectAllFilteredCommand(viewModel);
    }

    public IRelayCommand GetOpenDetailViewCommand(INavigationViewModel viewModel)
    {
        return new OpenDetailViewCommand(viewModel, _messenger);
    }

    //public IAsyncRelayCommand GetChangeMyodamConnectionCommand(INavigationViewModel viewModel)
    //{

    //}

    //public IAsyncRelayCommand GetExportSelectedCommand(INavigationViewModel viewModel)
    //{

    //}
}