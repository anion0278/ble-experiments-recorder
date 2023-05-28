using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Business.Device;
using BleRecorder.Business.Exception;
using BleRecorder.Common.Extensions;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.DataAccess.Repositories;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubjects;
using BleRecorder.UI.WPF.Calibration;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.Navigation.Commands;
using BleRecorder.UI.WPF.TestSubjects;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Helpers;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.Navigation
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly IGlobalExceptionHandler _exceptionHandler;
        private readonly INavigationItemViewModelFactory _navigationItemViewModelFactory;
        private readonly ObservableCollectionWithItemChangeNotification<INavigationItemViewModel> _navigationItems = new();
        private string _fullNameFilter;

        public ICollectionView TestSubjectsNavigationItems { get; }

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
            TestSubjectsNavigationItems = GetDefaultCollectionView(_navigationItems);
            _navigationItems.Add(new NavigationItemViewModel(testSubject, null!));
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
            INavigationItemViewModelFactory navigationItemViewModelFactory,
            INavigationViewModelCommandsFactory commandsFactory)
        {
            TestSubjectsNavigationItems = GetDefaultCollectionView(_navigationItems);
            _navigationItems.ItemPropertyChanged += ItemPropertyChanged; // TODO unsubscribe !!
            _navigationItems.CollectionChanged += NavigationItems_CollectionChanged;

            SelectAllFilteredCommand = commandsFactory.GetSelectAllFilteredCommand(this);
            DeselectAllFilteredCommand = commandsFactory.GetDeselectAllFilteredCommand(this);
            OpenDetailViewCommand = commandsFactory.GetOpenDetailViewCommand(this);
            ChangeBleRecorderConnectionCommand = commandsFactory.GetChangeBleRecorderConnectionCommand(this, bleRecorderManager, dialogService);
            ExportSelectedCommand = commandsFactory.GetExportSelectedCommand(
                this, bleRecorderManager, testSubjectRepository, dialogService, documentManager, fileManager);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _bleRecorderManager = bleRecorderManager;
            _exceptionHandler = exceptionHandler;
            _navigationItemViewModelFactory = navigationItemViewModelFactory;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderAvailabilityChanged;
            _bleRecorderManager.DevicePropertyChanged += OnBleRecorderPropertyChanged;
            _bleRecorderManager.DeviceErrorChanged += OnBleRecorderErrorChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnBleRecorderAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private void NavigationItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
            {
                OnPropertyChanged(nameof(TestSubjectsNavigationItems));
            }
        }

        private void ItemPropertyChanged(object? item, PropertyChangedEventArgs e)
        {
            if (e is { PropertyName: nameof(INavigationItemViewModel.IsSelectedForExport) })
            {
                OnPropertyChanged(nameof(TestSubjectsNavigationItems));
            }
        }

        public async Task LoadAsync()
        {
            var items = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => _navigationItemViewModelFactory.GetViewModel(ts));
            _navigationItems.AddRange(items);
            await DeviceCalibrationVm.LoadAsync();
        }

        private bool IsTestSubjectAccepted(object obj)
        {
            return obj is NavigationItemViewModel tsVM && tsVM.Model.FullName.ContainsCaseInsensitive(_fullNameFilter);
        }

        private async void OnBleRecorderErrorChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DeviceError));
            if (DeviceError != BleRecorderError.NoError)
            {
                await _exceptionHandler.HandleExceptionAsync(new DeviceErrorOccurredException(DeviceError));
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

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args) // TODO refactoring!
        {
            if (args.ViewModelName != nameof(TestSubjectDetailViewModel)) return;

            var item = _navigationItems.SingleOrDefault(f => f.Id == args.Id);
            if (item == null) return;

            _navigationItems.Remove(item);
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
        {
            if (args.ViewModelName != nameof(TestSubjectDetailViewModel)) return;

            var lookupItem = _navigationItems.SingleOrDefault(l => l.Id == args.Id);
            if (lookupItem != null) return;

            var ts = await _testSubjectRepository.GetByIdAsync(args.Id);
            _navigationItems.Add(_navigationItemViewModelFactory.GetViewModel(ts!));
        }
    }
}