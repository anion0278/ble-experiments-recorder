using CommunityToolkit.Mvvm.Input;
using BleRecorder.Business.Device;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;

namespace BleRecorder.UI.WPF.Navigation.Commands;

public interface INavigationViewModelCommandsFactory
{
    IRelayCommand GetSelectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetDeselectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetOpenDetailViewCommand(INavigationViewModel viewModel);
    IAsyncRelayCommand GetChangeBleRecorderConnectionCommand(INavigationViewModel viewModel, IBleRecorderManager bleRecorderManager, IMessageDialogService dialogService);
    IAsyncRelayCommand GetExportSelectedCommand(
        INavigationViewModel viewModel,
        IBleRecorderManager bleRecorderManager,
        ITestSubjectRepository testSubjectRepository,
        IMessageDialogService dialogService,
        IDocumentManager documentManager,
        IFileSystemManager fileManager);
}