using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.Common.Extensions;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.Navigation.Commands;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Commands;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.Navigation
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IMyodamManager _myodamManager;
        private readonly IDocumentManager _documentManager;
        private readonly IFileSystemManager _fileManager;
        private readonly IGlobalExceptionHandler _exceptionHandler;
        private readonly INavigationItemViewModelFactory _navigationItemViewModelFactory;
        private readonly ObservableCollection<INavigationTestSubjectItemViewModel> _navigationItems = new();
        private string _fullNameFilter;

        public ListCollectionView TestSubjectsNavigationItems { get; }

        public ICommand OpenDetailViewCommand { get; }

        public ICommand SelectAllFilteredCommand { get; }
        public ICommand DeselectAllFilteredCommand { get; }

        public IAsyncRelayCommand ChangeMyodamConnectionCommand { get; }

        public IAsyncRelayCommand ExportSelectedCommand { get; }

        public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;

        public int StimulatorBatteryPercentage => (int)(_myodamManager.MyodamDevice?.StimulatorBattery.Value ?? 0);

        public int ControllerBatteryPercentage => (int)(_myodamManager.MyodamDevice?.ControllerBattery.Value ?? 0);

        public MyodamError DeviceError => _myodamManager.MyodamDevice?.Error ?? MyodamError.NoError;

        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }

        // possible TODO: MVVM approach should provide Filtered ICollectionView and to let decide client (UI) how to use it (count)
        public int SelectedItemsCount => _navigationItems.Count(i => i.IsSelected);

        public string FullNameFilter
        {
            get => _fullNameFilter;
            set
            {
                _fullNameFilter = value;
                TestSubjectsNavigationItems.Filter = string.IsNullOrWhiteSpace(_fullNameFilter) ? null : IsTestSubjectAccepted;
            }
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        protected NavigationViewModel() : this(new TestSubject() { FirstName = "Name", LastName = "Surname" })
        {
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        protected NavigationViewModel(TestSubject testSubject)
        {
            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_navigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
            _navigationItems.Add(new NavigationTestSubjectItemViewModel(testSubject, null!));
        }

        public NavigationViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger,
            IMessageDialogService dialogService,
            IMyodamManager myodamManager,
            IDeviceCalibrationViewModel deviceCalibrationViewModel,
            IDocumentManager documentManager,
            IFileSystemManager fileManager,
            IGlobalExceptionHandler exceptionHandler,
            INavigationItemViewModelFactory navigationItemViewModelFactory,
            INavigationViewModelCommandsFactory commandsFactory)
        {
            TestSubjectsNavigationItems = (ListCollectionView)GetDefaultCollectionView(_navigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();

            ChangeMyodamConnectionCommand = new AsyncRelayCommand(ChangeMyodamConnectionAsync, CanChangeMyodamConnection);
            ExportSelectedCommand = new AsyncRelayCommand(ExportSelectedAsync, CanExportSelected);
            SelectAllFilteredCommand = commandsFactory.GetSelectAllFilteredCommand(this);
            DeselectAllFilteredCommand = commandsFactory.GetDeselectAllFilteredCommand(this);
            OpenDetailViewCommand = commandsFactory.GetOpenDetailViewCommand(this);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _myodamManager = myodamManager;
            _documentManager = documentManager;
            _fileManager = fileManager;
            _exceptionHandler = exceptionHandler;
            _navigationItemViewModelFactory = navigationItemViewModelFactory;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _myodamManager.MyodamAvailabilityChanged += OnMyodamAvailabilityChanged;
            _myodamManager.DevicePropertyChanged += OnMyodamPropertyChanged;
            _myodamManager.DeviceErrorChanged += OnMyodamErrorChanged;
            _myodamManager.MeasurementStatusChanged += OnMyodamAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }


        private bool IsTestSubjectAccepted(object obj)
        {
            return obj is NavigationTestSubjectItemViewModel tsVM && tsVM.Model.FullName.ContainsCaseInsensitive(_fullNameFilter);
        }

        private bool CanExportSelected()
        {
            return !_myodamManager.IsCurrentlyMeasuring && SelectedItemsCount > 0;
        }

        private async Task ExportSelectedAsync()
        {
            var subjects = _navigationItems
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

        private bool CanChangeMyodamConnection()
        {
            return MyodamAvailability != MyodamAvailabilityStatus.DisconnectedUnavailable && !_myodamManager.IsCurrentlyMeasuring;
        }

        public async Task ChangeMyodamConnectionAsync()
        {
            if (_myodamManager.IsCurrentlyMeasuring)
            {
                var result = await _dialogService.ShowOkCancelDialogAsync(
                    "Measurement is currently running. Are you sure you want to stop the measurement and disconnect from device?",
                    "Disconnect from device?");
                if (result != MessageDialogResult.OK) return;
            }
            if (_myodamManager.MyodamDevice != null && _myodamManager.MyodamDevice.IsConnected)
            {
                await _myodamManager.MyodamDevice.DisconnectAsync();
            }
            else await _myodamManager.ConnectMyodamAsync();
        }

        private async void OnMyodamErrorChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DeviceError));
            if (DeviceError != MyodamError.NoError)
            {
                await _exceptionHandler.HandleExceptionAsync(new DeviceErrorOccurredException(DeviceError));
            }
        }

        private void OnMyodamPropertyChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ControllerBatteryPercentage));
            OnPropertyChanged(nameof(StimulatorBatteryPercentage));
            OnPropertyChanged(nameof(MyodamAvailability));
        }

        private void OnMyodamAvailabilityChanged(object? o, EventArgs eventArgs)
        {
            OnMyodamPropertyChanged(this, EventArgs.Empty);
            ChangeMyodamConnectionCommand.NotifyCanExecuteChanged();
            ExportSelectedCommand.NotifyCanExecuteChanged();
        }

        public async Task LoadAsync()
        {
            var items = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => _navigationItemViewModelFactory.GetViewModel(ts));
            _navigationItems.AddRange(items);
            await DeviceCalibrationVm.LoadAsync();
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    var item = _navigationItems.SingleOrDefault(f => f.Id == args.Id);
                    if (item != null)
                    {
                        _navigationItems.Remove(item);
                    }
                    break;
            }
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    var lookupItem = _navigationItems.SingleOrDefault(l => l.Id == args.Id);
                    if (lookupItem == null)
                    {
                        var ts = await _testSubjectRepository.GetByIdAsync(args.Id);
                        _navigationItems.Add(_navigationItemViewModelFactory.GetViewModel(ts!));
                    }
                    else
                    if (lookupItem is NavigationTestSubjectItemViewModel nvm)
                    {
                        nvm.DisplayMember = args.DisplayMember;
                    }
                    break;
            }
        }
    }

    //internal class ChangeMyodamConnectionCommand : CustomAsyncRelayCommand
    //{
    //    public ChangeMyodamConnectionCommand() 
    //        : base(execute, canExecute)
    //    { }
    //}
        
}
