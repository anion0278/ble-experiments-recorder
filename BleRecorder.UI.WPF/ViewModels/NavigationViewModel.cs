using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IBleRecorderManager _bleRecorderManager;

        public ObservableCollection<NavigationItemViewModel> TestSubjects { get; } = new();

        public IAsyncRelayCommand ConnectBleRecorderCommand { get; }

        public BleRecorderAvailabilityStatus BleRecorderAvailability => _bleRecorderManager.BleRecorderAvailability;

        public NavigationViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger, 
            IBleRecorderManager bleRecorderManager, 
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ConnectBleRecorderCommand = asyncCommandFactory.Create(ConnectBleRecorder, CanConnectBleRecorder);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _bleRecorderManager = bleRecorderManager;

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderAvailabilityChanged;
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private void OnBleRecorderAvailabilityChanged(object? o, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(BleRecorderAvailability));
            ViewSynchronizationContext.Send(_ => ConnectBleRecorderCommand.NotifyCanExecuteChanged(), null);
        }

        private bool CanConnectBleRecorder()
        {
            return BleRecorderAvailability != BleRecorderAvailabilityStatus.DisconnectedUnavailable;
        }

        public async Task ConnectBleRecorder()
        {
            await _bleRecorderManager.ConnectBleRecorder();
        }

        public async Task LoadAsync()
        {
            var lookup = (await _testSubjectRepository.GetAllAsync())
                .Select(ts => new LookupItem
                {
                    Id = ts.Id,
                    DisplayMember = ts.FirstName + " " + ts.LastName // TODO check, refactor
                }).ToArray();
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
