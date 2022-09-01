﻿using System;
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
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.Win32;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _dialogService;
        private readonly IMyodamManager _myodamManager;
        private readonly IDocumentManager _documentManager;
        private readonly IFileSystemManager _fileManager;
        private readonly ObservableCollection<NavigationItemViewModel> _testSubjectsNavigationItems = new();

        public ListCollectionView TestSubjectsNavigationItems { get; } 

        public IAsyncRelayCommand ChangeMyodamConnectionCommand { get; }

        public IAsyncRelayCommand ExportSelectedCommand { get; }

        public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;

        public int StimulatorBatteryPercentage => (int)(_myodamManager.MyodamDevice?.StimulatorBattery.Value ?? 0);
        public int ControllerBatteryPercentage => (int)(_myodamManager.MyodamDevice?.ControllerBattery.Value ?? 0);

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
            IMyodamManager myodamManager,
            IAppConfigurationLoader configurationLoader,
            IDeviceCalibrationViewModel deviceCalibrationViewModel,
            IDocumentManager documentManager,
            IFileSystemManager fileManager,
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ChangeMyodamConnectionCommand = asyncCommandFactory.Create(ChangeMyodamConnectionAsync, CanChangeMyodamConnection);
            ExportSelectedCommand = asyncCommandFactory.Create(ExportSelectedAsync, CanExportSelected);

            _testSubjectRepository = testSubjectRepository;
            _messenger = messenger;
            _dialogService = dialogService;
            _myodamManager = myodamManager;
            _documentManager = documentManager;
            _fileManager = fileManager;
            DeviceCalibrationVm = deviceCalibrationViewModel;

            _myodamManager.MyodamAvailabilityChanged += OnMyodamAvailabilityChanged;
            _myodamManager.DevicePropertyChanged += OnMyodamPropertyChanged;
            _myodamManager.MeasurementStatusChanged += OnMyodamAvailabilityChanged; // yes, same handler
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));

            TestSubjectsNavigationItems = (ListCollectionView)CollectionViewSource.GetDefaultView(_testSubjectsNavigationItems);
            TestSubjectsNavigationItems.CustomSort = new NavigationAddItemViewModelRelationalComparer();
        }

        private bool CanExportSelected()
        {
            return true; // NOT DURING MEASUREMENT
        }

        private async Task ExportSelectedAsync()
        {
            var ts = await _testSubjectRepository.GetAllWithRelatedDataAsync();
            if (_fileManager.SaveSingleFileDialog("Export.xlsx", out var fileName))
            {
                await Task.Run(() => _documentManager.Export(fileName!, ts));
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
            ViewSynchronizationContext.Send(_ => ChangeMyodamConnectionCommand.NotifyCanExecuteChanged(), null);
        }

        private bool CanChangeMyodamConnection()
        {
            return MyodamAvailability != MyodamAvailabilityStatus.DisconnectedUnavailable && !_myodamManager.IsCurrentlyMeasuring;
        }

        public async Task ChangeMyodamConnectionAsync()
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
                await _myodamManager.MyodamDevice.DisconnectAsync();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else await _myodamManager.ConnectMyodamAsync();
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
