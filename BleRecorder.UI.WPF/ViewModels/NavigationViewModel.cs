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
using BleRecorder.Business.Device;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.ViewModels.Services;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly ObservableCollection<NavigationItemViewModel> _testSubjectsNavigationItems = new();

        public ListCollectionView TestSubjectsNavigationItems { get; } 

        public IAsyncRelayCommand ChangeBleRecorderConnectionCommand { get; }

        public BleRecorderAvailabilityStatus BleRecorderAvailability => _bleRecorderManager.BleRecorderAvailability;

        public int StimulatorBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.StimulatorBattery.Value ?? 0);
        public int ControllerBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.ControllerBattery.Value ?? 0);

        public IDeviceCalibrationViewModel DeviceCalibrationVm { get; }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        public NavigationViewModel()
        {
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        public NavigationViewModel(TestSubject testSubject)
        {
            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_testSubjectsNavigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
            _testSubjectsNavigationItems.Add(new NavigationItemViewModel(testSubject.Id, testSubject.FullName, null!));
            _testSubjectsNavigationItems.Add(new NavigationAddItemViewModel(null!));
        }

        public NavigationViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger, 
            IMessageDialogService dialogService,
            IBleRecorderManager bleRecorderManager,
            IAppConfigurationLoader configurationLoader,
            IDeviceCalibrationViewModel deviceCalibrationViewModel,
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ChangeBleRecorderConnectionCommand = asyncCommandFactory.Create(ChangeBleRecorderConnectionAsync, CanChangeBleRecorderConnection);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _bleRecorderManager = bleRecorderManager;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderAvailabilityChanged;
            _bleRecorderManager.DevicePropertyChanged += OnBleRecorderPropertyChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnBleRecorderAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));

            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_testSubjectsNavigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
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
            ViewSynchronizationContext.Send(_ => ChangeBleRecorderConnectionCommand.NotifyCanExecuteChanged(), null);
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
                .Select(ts => new NavigationItemViewModel(ts.Id, ts.FullName, _messenger));
            _testSubjectsNavigationItems.AddRange(items);
            _testSubjectsNavigationItems.Add(new NavigationAddItemViewModel(_messenger));

            await DeviceCalibrationVm.LoadAsync();
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    var item = _testSubjectsNavigationItems.SingleOrDefault(f => f.Id == args.Id);
                    if (item != null)
                    {
                        _testSubjectsNavigationItems.Remove(item);
                    }
                    break;
            }
        }

        private void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    var lookupItem = _testSubjectsNavigationItems.SingleOrDefault(l => l.Id == args.Id);
                    if (lookupItem == null)
                    {
                        _testSubjectsNavigationItems.Add(new NavigationItemViewModel(args.Id, args.DisplayMember, _messenger));
                    }
                    else
                    {
                        lookupItem.DisplayMember = args.DisplayMember;
                    }
                    break;
            }
        }
    }
}
