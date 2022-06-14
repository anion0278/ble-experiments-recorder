﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BleRecorder.Business.Device;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Data.Lookups;
using BleRecorder.UI.WPF.Event;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly IMessenger _messenger;
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly IBluetoothManager _bluetoothManager;
        private readonly ITestSubjectLookupDataService _testSubjectLookupService;

        public ObservableCollection<NavigationItemViewModel> TestSubjects { get; } = new();

        public ICommand ConnectBleRecorderCommand { get; }

        public DeviceStatus BleRecorderStatus => _bleRecorderManager.BleRecorderStatus;

        public NavigationViewModel(ITestSubjectLookupDataService testSubjectLookupService, IMessenger messenger, IBleRecorderManager bleRecorderManager)
        {
            ConnectBleRecorderCommand = new AsyncRelayCommand(ConnectBleRecorder, () => true);
            _testSubjectLookupService = testSubjectLookupService;
            _messenger = messenger;
            _bleRecorderManager = bleRecorderManager;
            _bleRecorderManager.BleRecorderStatusChanged += (_, _) => { OnPropertyChanged(nameof(BleRecorderStatus)); };
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        public async Task ConnectBleRecorder()
        {
            try
            {
                await _bleRecorderManager.ConnectBleRecorder();
            }
            catch (Exception ex)
            {

            }
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
