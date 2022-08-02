using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Views.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IMyodamManager _myodamManager;

        public ObservableCollection<NavigationItemViewModel> TestSubjects { get; } = new();

        public IAsyncRelayCommand ChangeMyodamConnectionCommand { get; }

        public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;

        public int StimulatorBatteryPercentage => (int)(_myodamManager.MyodamDevice?.StimulatorBattery.Value ?? 0);
        public int ControllerBatteryPercentage => (int)(_myodamManager.MyodamDevice?.ControllerBattery.Value ?? 0);

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
            IMyodamManager myodamManager, 
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ChangeMyodamConnectionCommand = asyncCommandFactory.Create(ChangeMyodamConnection, CanChangeMyodamConnection);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _myodamManager = myodamManager;

            _myodamManager.MyodamAvailabilityChanged += OnMyodamAvailabilityChanged;
            _myodamManager.DevicePropertyChanged += OnMyodamPropertyChanged;
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
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
            ViewSynchronizationContext.Send(_ => ChangeMyodamConnectionCommand.NotifyCanExecuteChanged(), null);
        }

        private bool CanChangeMyodamConnection()
        {
            return MyodamAvailability != MyodamAvailabilityStatus.DisconnectedUnavailable;
        }

        public async Task ChangeMyodamConnection()
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
                await _myodamManager.MyodamDevice.Disconnect();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else await _myodamManager.ConnectMyodam();
        }

        public async Task LoadAsync()
        {
            // TODO check, refactor
            var lookup = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => new LookupItem { Id = ts.Id, DisplayMember = ts.FullName })
                .ToArray();
            TestSubjects.Clear();
            foreach (var item in lookup)
            {
                TestSubjects.Add(new NavigationItemViewModel(
                    item.Id, 
                    item.DisplayMember, 
                    nameof(TestSubjectDetailViewModel), 
                    _messenger));
            }
        }


        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    AfterDetailDeleted(TestSubjects, args);
                    break;
            }
        }

        private void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
        {
            switch (args.ViewModelName)
            {
                case nameof(TestSubjectDetailViewModel):
                    AfterDetailSaved(TestSubjects, args);
                    break;
            }
        }

        private void AfterDetailDeleted(ObservableCollection<NavigationItemViewModel> items, AfterDetailDeletedEventArgs args)
        {
            var item = items.SingleOrDefault(f => f.Id == args.Id);
            if (item != null)
            {
                items.Remove(item);
            }
        }

        private void AfterDetailSaved(ObservableCollection<NavigationItemViewModel> items, AfterDetailSavedEventArgs args)
        {
            var lookupItem = items.SingleOrDefault(l => l.Id == args.Id);
            if (lookupItem == null)
            {
                items.Add(new NavigationItemViewModel(args.Id, args.DisplayMember,
                  args.ViewModelName,
                  _messenger));
            }
            else
            {
                lookupItem.DisplayMember = args.DisplayMember;
            }
        }
    }
}
