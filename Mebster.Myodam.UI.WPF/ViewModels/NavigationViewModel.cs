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
using Mebster.Myodam.UI.WPF.Data.Lookups;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly IMessenger _messenger;
        private readonly IMyodamManager _myodamManager;
        private readonly ITestSubjectLookupDataService _testSubjectLookupService;

        public ObservableCollection<NavigationItemViewModel> TestSubjects { get; } = new();

        private SynchronizationContext ViewSynchronizationContext { get; }

        public IAsyncRelayCommand ConnectMyodamCommand { get; }

        public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;


        public NavigationViewModel(
            ITestSubjectLookupDataService testSubjectLookupService, 
            IMessenger messenger, 
            IMyodamManager myodamManager, 
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ViewSynchronizationContext = SynchronizationContext.Current!;
            ConnectMyodamCommand = asyncCommandFactory.Create(ConnectMyodam, CanConnectMyodam);

            _testSubjectLookupService = testSubjectLookupService;
            _messenger = messenger;
            _myodamManager = myodamManager;
            _myodamManager.MyodamAvailabilityChanged += OnMyodamAvailabilityChanged;
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private void OnMyodamAvailabilityChanged(object? o, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(MyodamAvailability));
            ViewSynchronizationContext.Send(_ => ConnectMyodamCommand.NotifyCanExecuteChanged(), null);
        }

        private bool CanConnectMyodam()
        {
            return MyodamAvailability != MyodamAvailabilityStatus.DisconnectedUnavailable;
        }

        public async Task ConnectMyodam()
        {
            await _myodamManager.ConnectMyodam();
        }

        public async Task LoadAsync()
        {
            var lookup = await _testSubjectLookupService.GetTestSubjectLookupAsync();
            TestSubjects.Clear();
            foreach (var item in lookup)
            {
                TestSubjects.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
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

        private void AfterDetailDeleted(ObservableCollection<NavigationItemViewModel> items,
          AfterDetailDeletedEventArgs args)
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
