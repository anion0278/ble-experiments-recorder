using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using BleRecorder.Business.Device;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Commands;
using BleRecorder.UI.WPF.ViewModels.Services;

namespace BleRecorder.UI.WPF.Navigation.Commands;

public class ExportSelectedCommand : CustomAsyncRelayCommand
{
    private readonly INavigationViewModel _viewModel;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly ITestSubjectRepository _testSubjectRepository;
    private readonly IMessageDialogService _dialogService;
    private readonly IDocumentManager _documentManager;
    private readonly IFileSystemManager _fileManager;

    public ExportSelectedCommand(
        INavigationViewModel viewModel, 
        IBleRecorderManager bleRecorderManager, 
        ITestSubjectRepository testSubjectRepository,
        IMessageDialogService dialogService,
        IDocumentManager documentManager,
        IFileSystemManager fileManager)
    {
        _viewModel = viewModel;
        _bleRecorderManager = bleRecorderManager;
        _testSubjectRepository = testSubjectRepository;
        _dialogService = dialogService;
        _documentManager = documentManager;
        _fileManager = fileManager;
    }

    protected override AsyncRelayCommand CreateCommand()
    {
        return new AsyncRelayCommand(ExportSelectedAsync, CanExportSelected);
    }

    private bool CanExportSelected()
    {
        return !_bleRecorderManager.IsCurrentlyMeasuring && _viewModel.SelectedItemsCount > 0;
    }

    private async Task ExportSelectedAsync()
    {
        var subjects = _viewModel.TestSubjectsNavigationItems
            .OfType<INavigationItemViewModel>()
            .Where(item => item.IsSelected)
            .Select(item => item.Model).ToArray();

        //// TODO optimize query 
        var reloadedSubjects = await _testSubjectRepository.GetAllWithRelatedDataAsync();

        //foreach (var ts in subjects) // Does not work the same way !! measurements are not loaded at all
        //{
        //    await _testSubjectRepository.ReloadAsync(ts);
        //}

        if (_dialogService.SaveSingleFileDialog("Export.xlsx", out var filePath))
        {
            await Task.Run(() => _documentManager.Export(filePath!, subjects));
            _dialogService.OpenOrShowDir(_fileManager.GetFileDir(filePath));
        }
    }
}