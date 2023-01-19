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
using DocumentFormat.OpenXml.Bibliography;
using BleRecorder.Business.Device;
using BleRecorder.Business.Exception;
using BleRecorder.Common.Extensions;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.ViewModels.Services;
using BleRecorder.UI.WPF.Views.Resouces;
using Microsoft.AppCenter;
using Microsoft.Win32;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly IDocumentManager _documentManager;
        private readonly IFileSystemManager _fileManager;
        private readonly IGlobalExceptionHandler _exceptionHandler;
        private readonly ObservableCollection<NavigationTestSubjectItemViewModel> _navigationItems = new();
        private string _fullNameFilter;

        public ListCollectionView TestSubjectsNavigationItems { get; }

        public ICommand OpenDetailViewCommand { get; }

        public ICommand SelectAllFilteredCommand { get; }
        public ICommand DeselectAllFilteredCommand { get; }

        public IAsyncRelayCommand ChangeBleRecorderConnectionCommand { get; }

        public IAsyncRelayCommand ExportSelectedCommand { get; }

        public BleRecorderAvailabilityStatus BleRecorderAvailability => _bleRecorderManager.BleRecorderAvailability;

        public int StimulatorBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.StimulatorBattery.Value ?? 0);

        public int ControllerBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.ControllerBattery.Value ?? 0);

        public BleRecorderError DeviceError => _bleRecorderManager.BleRecorderDevice?.Error ?? BleRecorderError.NoError;

        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }

        // possible TODO: MVVM approach should provide Filtered ICollectionView and to let decide client (UI) how to use it (count)
        public int SelectedItemsCount => _navigationItems
            .Count(i => i.IsSelected);

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
        }

        public NavigationViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger,
            IMessageDialogService dialogService,
            IBleRecorderManager bleRecorderManager,
            IDeviceCalibrationViewModel deviceCalibrationViewModel,
            IDocumentManager documentManager,
            IFileSystemManager fileManager,
            IGlobalExceptionHandler exceptionHandler,
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_navigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();

            ChangeBleRecorderConnectionCommand = asyncCommandFactory.Create(ChangeBleRecorderConnectionAsync, CanChangeBleRecorderConnection);
            ExportSelectedCommand = asyncCommandFactory.Create(ExportSelectedAsync, CanExportSelected);
            SelectAllFilteredCommand = new RelayCommand(() => TestSubjectsNavigationItems.Cast<NavigationTestSubjectItemViewModel>().ForEach(i => i.IsSelected = true));
            DeselectAllFilteredCommand = new RelayCommand(() => TestSubjectsNavigationItems.Cast<NavigationTestSubjectItemViewModel>().ForEach(i => i.IsSelected = false));

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _bleRecorderManager = bleRecorderManager;
            _documentManager = documentManager;
            _fileManager = fileManager;
            _exceptionHandler = exceptionHandler;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderAvailabilityChanged;
            _bleRecorderManager.DevicePropertyChanged += OnBleRecorderPropertyChanged;
            _bleRecorderManager.DeviceErrorChanged += OnBleRecorderErrorChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnBleRecorderAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }


        private bool IsTestSubjectAccepted(object obj)
        {
            return obj is NavigationTestSubjectItemViewModel tsVM && tsVM.Model.FullName.ContainsCaseInsensitive(_fullNameFilter);
        }

        private void OnOpenDetailViewExecute()
        {
            _messenger.Send(new OpenDetailViewEventArgs
            {
                Id = -999, // for new item
                ViewModelName = nameof(TestSubjectDetailViewModel)
            });
        }

        private async void OnBleRecorderErrorChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DeviceError));
            if (DeviceError != BleRecorderError.NoError)
            {
                await _exceptionHandler.HandleExceptionAsync(new DeviceErrorOccurredException(DeviceError));
            }
        }

        private bool CanExportSelected()
        {
            OnPropertyChanged(nameof(SelectedItemsCount));
            return !_bleRecorderManager.IsCurrentlyMeasuring && SelectedItemsCount > 0;
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

        private void OnBleRecorderPropertyChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ControllerBatteryPercentage));
            OnPropertyChanged(nameof(StimulatorBatteryPercentage));
            OnPropertyChanged(nameof(BleRecorderAvailability));
        }

        private void OnBleRecorderAvailabilityChanged(object? o, EventArgs eventArgs)
        {
            OnBleRecorderPropertyChanged(this, EventArgs.Empty);
            ChangeBleRecorderConnectionCommand.NotifyCanExecuteChanged();
            ExportSelectedCommand.NotifyCanExecuteChanged();
        }

        private bool CanChangeBleRecorderConnection()
        {
            return BleRecorderAvailability != BleRecorderAvailabilityStatus.DisconnectedUnavailable && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        public async Task ChangeBleRecorderConnectionAsync()
        {
            if (_bleRecorderManager.IsCurrentlyMeasuring)
            {
                var result = await _dialogService.ShowOkCancelDialogAsync(
                    "Measurement is currently running. Are you sure you want to stop the measurement and disconnect from device?",
                    "Disconnect from device?");
                if (result != MessageDialogResult.OK) return;
            }
            if (_bleRecorderManager.BleRecorderDevice != null && _bleRecorderManager.BleRecorderDevice.IsConnected)
            {
                await _bleRecorderManager.BleRecorderDevice.DisconnectAsync();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else await _bleRecorderManager.ConnectBleRecorderAsync();
        }

        public async Task LoadAsync()
        {
            var items = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => new NavigationTestSubjectItemViewModel(ts, _messenger));
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
