using CommunityToolkit.Mvvm.Input;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Navigation.Commands;

public interface INavigationViewModelCommandsFactory
{
    IRelayCommand GetSelectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetDeselectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetOpenDetailViewCommand(INavigationViewModel viewModel);
    //IAsyncRelayCommand GetChangeBleRecorderConnectionCommand(INavigationViewModel viewModel);
    //IAsyncRelayCommand GetExportSelectedCommand(INavigationViewModel viewModel);
}