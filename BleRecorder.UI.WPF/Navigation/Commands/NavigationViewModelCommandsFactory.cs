using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Business.Device;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.DataAccess.Repositories;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;

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

    public IAsyncRelayCommand GetChangeBleRecorderConnectionCommand(INavigationViewModel viewModel, IBleRecorderManager bleRecorderManager, IMessageDialogService dialogService)
    {
        return new ChangeBleRecorderConnectionCommand(viewModel, bleRecorderManager, dialogService);
    }

    public IAsyncRelayCommand GetExportSelectedCommand(
        INavigationViewModel viewModel,
        IBleRecorderManager bleRecorderManager,
        ITestSubjectRepository testSubjectRepository,
        IMessageDialogService dialogService,
        IDocumentManager documentManager,
        IFileSystemManager fileManager)
    {
        return new ExportSelectedItemsCommand(
            viewModel,
            bleRecorderManager,
            testSubjectRepository,
            dialogService,
            documentManager,
            fileManager);
    }
}