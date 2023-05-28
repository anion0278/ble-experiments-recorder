using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.DataAccess.Repositories;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

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

    public IAsyncRelayCommand GetChangeMyodamConnectionCommand(INavigationViewModel viewModel, IMyodamManager myodamManager, IMessageDialogService dialogService)
    {
        return new ChangeMyodamConnectionCommand(viewModel, myodamManager, dialogService);
    }

    public IAsyncRelayCommand GetExportSelectedCommand(
        INavigationViewModel viewModel,
        IMyodamManager myodamManager,
        ITestSubjectRepository testSubjectRepository,
        IMessageDialogService dialogService,
        IDocumentManager documentManager,
        IFileSystemManager fileManager)
    {
        return new ExportSelectedItemsCommand(
            viewModel,
            myodamManager,
            testSubjectRepository,
            dialogService,
            documentManager,
            fileManager);
    }
}