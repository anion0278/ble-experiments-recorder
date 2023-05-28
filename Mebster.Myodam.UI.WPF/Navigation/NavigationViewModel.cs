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
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.Common.Extensions;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.DataAccess.Repositories;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Calibration;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.Navigation.Commands;
using Mebster.Myodam.UI.WPF.TestSubjects;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Helpers;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.Navigation
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMyodamManager _myodamManager;
        private readonly IGlobalExceptionHandler _exceptionHandler;
        private readonly INavigationItemViewModelFactory _navigationItemViewModelFactory;
        private readonly ObservableCollectionWithItemChangeNotification<INavigationItemViewModel> _navigationItems = new();
        private string _fullNameFilter;

        public ICollectionView TestSubjectsNavigationItems { get; }

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
            IMyodamManager myodamManager,
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
            ChangeMyodamConnectionCommand = commandsFactory.GetChangeMyodamConnectionCommand(this, myodamManager, dialogService);
            ExportSelectedCommand = commandsFactory.GetExportSelectedCommand(
                this, myodamManager, testSubjectRepository, dialogService, documentManager, fileManager);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _myodamManager = myodamManager;
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