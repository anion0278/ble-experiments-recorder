using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.DataAccess.Repositories;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

public interface INavigationViewModelCommandsFactory
{
    IRelayCommand GetSelectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetDeselectAllFilteredCommand(INavigationViewModel viewModel);
    IRelayCommand GetOpenDetailViewCommand(INavigationViewModel viewModel);
    IAsyncRelayCommand GetChangeMyodamConnectionCommand(INavigationViewModel viewModel, IMyodamManager myodamManager, IMessageDialogService dialogService);
    IAsyncRelayCommand GetExportSelectedCommand(
        INavigationViewModel viewModel,
        IMyodamManager myodamManager,
        ITestSubjectRepository testSubjectRepository,
        IMessageDialogService dialogService,
        IDocumentManager documentManager,
        IFileSystemManager fileManager);
}