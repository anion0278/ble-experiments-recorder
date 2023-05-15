using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.Navigation.Commands;
using BleRecorder.UI.WPF.ViewModels;
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
        private readonly ObservableCollectionEX<INavigationItemViewModel> _navigationItems = new();
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
            _navigationItems.CollectionChanged += _navigationItems_CollectionChanged; 

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

        private void _navigationItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
            {
                OnPropertyChanged(nameof(SelectedItemsCount));
            }
        }

        private void ItemPropertyChanged(object item, PropertyChangedEventArgs e)
        {
            if (e is { PropertyName: nameof(INavigationItemViewModel.IsSelected)})
            {
                OnPropertyChanged(nameof(SelectedItemsCount));
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
                    if (lookupItem is NavigationItemViewModel nvm)
                    {
                        nvm.DisplayMember = args.DisplayMember;
                    }
                    break;
            }
        }
    }
}

public dehandate void ListedItemPropertyChangedEventHandler(object Item, PropertyChangedEventArgs e);
public class ObservableCollectionEX<T> : ObservableCollection<T>
{
    #region Constructors
    public ObservableCollectionEX() : base()
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }
    public ObservableCollectionEX(IEnumerable<T> c) : base(c)
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }
    public ObservableCollectionEX(List<T> l) : base(l)
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }

    #endregion

    public new void Clear()
    {
        foreach (var item in this)
            if (item is INotifyPropertyChanged i)
                i.PropertyChanged -= Element_PropertyChanged;
        base.Clear();
    }

    private void ObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (var item in e.OldItems)
                if (item != null && item is INotifyPropertyChanged i)
                    i.PropertyChanged -= Element_PropertyChanged;


        if (e.NewItems != null)
            foreach (var item in e.NewItems)
                if (item != null && item is INotifyPropertyChanged i)
                {
                    i.PropertyChanged -= Element_PropertyChanged;
                    i.PropertyChanged += Element_PropertyChanged;
                }
    }

    private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(sender, e);
    }

    public ListedItemPropertyChangedEventHandler ItemPropertyChanged;
}
