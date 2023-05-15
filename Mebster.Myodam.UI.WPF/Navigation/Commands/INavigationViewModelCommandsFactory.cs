using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

public interface INavigationViewModelCommandsFactory
{
    IRelayCommand GetSelectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetDeselectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetOpenDetailViewCommand(INavigationViewModel viewModel);
    //IAsyncRelayCommand GetChangeMyodamConnectionCommand(INavigationViewModel viewModel);
    //IAsyncRelayCommand GetExportSelectedCommand(INavigationViewModel viewModel);
}