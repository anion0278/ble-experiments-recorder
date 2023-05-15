using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Navigation.Commands;

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

    //public IAsyncRelayCommand GetChangeBleRecorderConnectionCommand(INavigationViewModel viewModel)
    //{

    //}

    //public IAsyncRelayCommand GetExportSelectedCommand(INavigationViewModel viewModel)
    //{

    //}
}