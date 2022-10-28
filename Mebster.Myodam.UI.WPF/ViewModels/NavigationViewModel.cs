using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.AppCenter;
using Microsoft.Win32;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.ViewModels
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
        private readonly ObservableCollection<NavigationAddTestSubjectItemViewModel> _navigationItems = new();

        public ListCollectionView TestSubjectsNavigationItems { get; }

        public IAsyncRelayCommand ChangeMyodamConnectionCommand { get; }

        public IAsyncRelayCommand ExportSelectedCommand { get; }

        public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;

        public int StimulatorBatteryPercentage => (int)(_myodamManager.MyodamDevice?.StimulatorBattery.Value ?? 0);
        public int ControllerBatteryPercentage => (int)(_myodamManager.MyodamDevice?.ControllerBattery.Value ?? 0);

        public MyodamError DeviceError => _myodamManager.MyodamDevice?.Error ?? MyodamError.NoError;

        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        public NavigationViewModel() : this(new TestSubject() { FirstName = "Name", LastName = "Surname" })
        {
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        public NavigationViewModel(TestSubject testSubject)
        {
            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_navigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
            _navigationItems.Add(new NavigationTestSubjectItemViewModel(testSubject, null!));
            _navigationItems.Add(new NavigationAddTestSubjectItemViewModel(null!));
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
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ChangeMyodamConnectionCommand = asyncCommandFactory.Create(ChangeMyodamConnectionAsync, CanChangeMyodamConnection);
            ExportSelectedCommand = asyncCommandFactory.Create(ExportSelectedAsync, CanExportSelected);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _myodamManager = myodamManager;
            _documentManager = documentManager;
            _fileManager = fileManager;
            _exceptionHandler = exceptionHandler;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _myodamManager.MyodamAvailabilityChanged += OnMyodamAvailabilityChanged;
            _myodamManager.DevicePropertyChanged += OnMyodamPropertyChanged;
            _myodamManager.DeviceErrorChanged += OnMyodamErrorChanged;
            _myodamManager.MeasurementStatusChanged += OnMyodamAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));

            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_navigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
        }

        private async void OnMyodamErrorChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DeviceError));
            if (DeviceError != MyodamError.NoError)
            {
                await _exceptionHandler.HandleExceptionAsync(new DeviceErrorOccurredException(DeviceError));
            }
        }

        private bool CanExportSelected()
        {
            return !_myodamManager.IsCurrentlyMeasuring && _navigationItems
                .OfType<NavigationTestSubjectItemViewModel>()
                .Count(item => item.IsSelected) > 0;
        }

        private async Task ExportSelectedAsync()
        {
            var subjects = _navigationItems
                .OfType<NavigationTestSubjectItemViewModel>()
                .Where(item => item.IsSelected)
                .Select(item => item.Model).ToArray();

            //// TODO optimize query 
            var reloadedSubjects = await _testSubjectRepository.GetAllWithRelatedDataAsync();

            //foreach (var ts in subjects) // Does not work the same way !! measurements are not loaded at all
            //{
            //    await _testSubjectRepository.ReloadAsync(ts);
            //}

            if (_fileManager.SaveSingleFileDialog("Export.xlsx", out var filePath))
            {
                await Task.Run(() => _documentManager.Export(filePath!, subjects));
                _fileManager.OpenOrShowDir(_fileManager.GetFileDir(filePath));
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
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else await _myodamManager.ConnectMyodamAsync();
        }

        public async Task LoadAsync()
        {
            var items = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => new NavigationTestSubjectItemViewModel(ts, _messenger));
            _navigationItems.AddRange(items);
            _navigationItems.Add(new NavigationAddTestSubjectItemViewModel(_messenger));
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
                        _navigationItems.Add(new NavigationTestSubjectItemViewModel(ts!, _messenger));
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
}
