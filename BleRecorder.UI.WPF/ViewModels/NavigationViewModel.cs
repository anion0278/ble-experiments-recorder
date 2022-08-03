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
using BleRecorder.Business.Device;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Views.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IBleRecorderManager _bleRecorderManager;

        public ObservableCollection<NavigationItemViewModel> _testSubjectsNavigationItems { get; } = new();
        public ListCollectionView TestSubjectsNavigationItems { get; } 

        public IAsyncRelayCommand ChangeBleRecorderConnectionCommand { get; }

        public BleRecorderAvailabilityStatus BleRecorderAvailability => _bleRecorderManager.BleRecorderAvailability;

        public int StimulatorBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.StimulatorBattery.Value ?? 0);
        public int ControllerBatteryPercentage => (int)(_bleRecorderManager.BleRecorderDevice?.ControllerBattery.Value ?? 0);

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public NavigationViewModel()
        {
        }

        public NavigationViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger, 
            IMessageDialogService dialogService,
            IBleRecorderManager bleRecorderManager, 
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ChangeBleRecorderConnectionCommand = asyncCommandFactory.Create(ChangeBleRecorderConnection, CanChangeBleRecorderConnection);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _bleRecorderManager = bleRecorderManager;

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderAvailabilityChanged;
            _bleRecorderManager.DevicePropertyChanged += OnBleRecorderPropertyChanged;
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
            return BleRecorderAvailability != BleRecorderAvailabilityStatus.DisconnectedUnavailable;
        }

        public async Task ChangeBleRecorderConnection()
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
                await _bleRecorderManager.BleRecorderDevice.Disconnect();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else await _bleRecorderManager.ConnectBleRecorder();
        }

        public async Task LoadAsync()
        {
            var items = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => new NavigationItemViewModel(ts.Id, ts.FullName, _messenger));
            _testSubjectsNavigationItems.AddRange(items);
            _testSubjectsNavigationItems.Add(new NavigationAddItemViewModel(_messenger));
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
