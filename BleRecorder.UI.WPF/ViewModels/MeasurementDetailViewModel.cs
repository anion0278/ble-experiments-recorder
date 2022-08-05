using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using AutoMapper;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;
using Swordfish.NET.Collections.Auxiliary;
using WinRT;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly IMapper _mapper;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IDateTimeService _dateTimeService;

        public ChartValues<MeasuredValue> MeasuredValues { get; set; } = new();

        public override string Title => string.IsNullOrWhiteSpace(Measurement.Title) ? "(New measurement)" : Measurement.Title;

        [Required]
        [StringLength(30, MinimumLength = 1)]
        [Display(Name = "Short description")]
        [AlsoNotifyFor(nameof(Title))]
        public string MeasurementDescription
        {
            get => Measurement.Title;
            set => Measurement.Title = value;
        }

        public string? Notes
        {
            get => Measurement.Notes;
            set => Measurement.Notes = value;
        }

        public DateTimeOffset? Date
        {
            get => Measurement.Date;
            set => Measurement.Date = value;
        }

        public MeasurementType Type
        {
            get => Measurement.Type;
            set => Measurement.Type = value;
        }

        public PositionDuringMeasurement Position
        {
            get => Measurement.PositionDuringMeasurement;
            set => Measurement.PositionDuringMeasurement = value;
        }

        public MeasurementSite Site
        {
            get => Measurement.SiteDuringMeasurement;
            set => Measurement.SiteDuringMeasurement = value;
        }

        public StimulationParametersViewModel StimulationParametersVm { get; private set; }

        public MechanismParametersViewModel MechanismParametersVm { get; private set; }

        public Measurement Measurement { get; private set; }

        public IRelayCommand StartMeasurementCommand { get; }
        public IRelayCommand StopMeasurementCommand { get; }
        public IRelayCommand CleanRecordedDataCommand { get; set; }


        /// <summary>
        /// Design-time ctor    
        /// </summary>
        public MeasurementDetailViewModel() : base(null!, null!, null!)
        {
        }

        public MeasurementDetailViewModel(IMessenger messenger,
            IMessageDialogService dialogService,
            IBleRecorderManager bleRecorderManager,
            IMapper mapper,
            IMeasurementRepository measurementRepository,
            IDateTimeService dateTimeService) : base(messenger, dialogService, bleRecorderManager)
        {
            _bleRecorderManager = bleRecorderManager;
            _mapper = mapper;
            _measurementRepository = measurementRepository;
            _dateTimeService = dateTimeService;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, StopMeasurementCanExecute);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => !_bleRecorderManager.IsCurrentlyMeasuring);

            MeasuredValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private bool StopMeasurementCanExecute()
        {
            return _bleRecorderManager.IsCurrentlyMeasuring && !_bleRecorderManager.BleRecorderDevice!.IsCalibrating;
        }

        protected override bool OnDeleteCanExecute()
        {
            return !StopMeasurementCanExecute();
        }

        protected override void UnsubscribeOnClosing()
        {
            MechanismParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            StimulationParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            PropertyChanged -= OnPropertyChangedEventHandler;

            MeasuredValues.CollectionChanged -= OnForceValuesChanged;
            _bleRecorderManager.BleRecorderAvailabilityChanged -= OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;

            Messenger.Unregister<AfterDetailSavedEventArgs>(this);
            Messenger.Unregister<AfterDetailDeletedEventArgs>(this);
        }

        private void OnPropertyChangedEventHandler(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            MechanismParametersVm.CopyAdjustmentValuesTo(Measurement.AdjustmentsDuringMeasurement!);

            HasChanges = _measurementRepository.HasChanges();
        }

        private void OnForceValuesChanged(object? o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(nameof(MeasuredValues));
        }

        private void OnMeasurementStatusChanged(object? sender, EventArgs e)
        {
            // TODO solve case when device is null (was already disconnected and may cause memory leak)
            if (!_bleRecorderManager.IsCurrentlyMeasuring && _bleRecorderManager.BleRecorderDevice != null) //_bleRecorderManager.BleRecorderDevice != null
            {
                _bleRecorderManager.BleRecorderDevice.NewValueReceived -= OnNewValueReceived;
            }

            ViewSynchronizationContext.Send(_ =>
            {
                StartMeasurementCommand.NotifyCanExecuteChanged();
                StopMeasurementCommand.NotifyCanExecuteChanged();
                CleanRecordedDataCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                if (_bleRecorderManager.IsCurrentlyMeasuring) return;

                StopMeasurementCommand.NotifyCanExecuteChanged();
                if (_bleRecorderManager.BleRecorderDevice is not null && _bleRecorderManager.BleRecorderDevice.IsConnected) return;

                MessageDialogService.ShowInfoDialogAsync("Measurement interrupted due to device disconnection!");
            }, null);
        }

        private bool StartMeasurementCanExecute()
        {
            return _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        private void OnBleRecorderStatusChanged(object? o, EventArgs eventArgs)
        {
            ViewSynchronizationContext.Send(_ => StartMeasurementCommand.NotifyCanExecuteChanged(), null);
        }

        private async void CleanRecordedData()
        {
            if (MeasuredValues.Count <= 0) return;

            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to remove measurement data?",
                "Delete data?");
            if (result == MessageDialogResult.OK)
            {
                MeasuredValues.Clear();
            }
            UpdateMeasurementForceData();
        }

        public override async Task LoadAsync(int measurementId, object argsData)
        {
            var ts = (TestSubject)argsData;
            Measurement = (measurementId > 0
                ? await _measurementRepository.GetByIdAsync(measurementId)
                : await CreateNewMeasurement(ts))!;

            Measurement.ParametersDuringMeasurement ??= (StimulationParameters)ts.CustomizedParameters.Clone();
            StimulationParametersVm = new StimulationParametersViewModel(Measurement.ParametersDuringMeasurement);
            StimulationParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            Measurement.AdjustmentsDuringMeasurement ??= (DeviceMechanicalAdjustments)ts.CustomizedAdjustments.Clone();
            MechanismParametersVm = new MechanismParametersViewModel(new MechanismParameters(Measurement.AdjustmentsDuringMeasurement), _mapper);
            MechanismParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            MeasuredValues.AddRange(Measurement.ForceData);
            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
        }


        private async Task<Measurement> CreateNewMeasurement(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement
            {
                ForceData = new List<MeasuredValue>(),
                TestSubject = (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!
            };
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurement()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to start measurement with current parameters listed in this page (they may differ from user-specific parameter settings)?",
                "Start measurement?");
            if (result != MessageDialogResult.OK) return;

            if (MeasuredValues.Count > 0)
            {
                result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "Measurement already contains data. Starting a new measurement will erase the existing data. Do you want to continue?",
                    "Delete measurement data?");
                if (result != MessageDialogResult.OK) return;
            }

            MeasuredValues.Clear();
            Date = _dateTimeService.Now;
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived += OnNewValueReceived;
            await _bleRecorderManager.BleRecorderDevice.StartMeasurement(Measurement.ParametersDuringMeasurement!, Type);
        }

        private void OnNewValueReceived(object? _, MeasuredValue value)
        {
            MeasuredValues.Add(value);
        }

        public async Task StopMeasurement()
        {
            if (_bleRecorderManager.BleRecorderDevice != null)
            {
                await _bleRecorderManager.BleRecorderDevice.StopMeasurement();
            }
        }

        protected override async void OnDeleteExecute() // TODO into base class, since TS OnDeleteExecute is almost same. Abstract a single main repository
        {
            if (Id < 0)
            {
                OnCloseDetailViewExecute();
                return;
            }

            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the measurement {Measurement.Title}?", "Question");

            if (result != MessageDialogResult.OK) return;

            _measurementRepository.Remove(Measurement);
            await _measurementRepository.SaveAsync();
            RaiseDetailDeletedEvent(Measurement.Id);
        }

        protected override async void OnSaveExecute()
        {
            UpdateMeasurementForceData();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }

        private void UpdateMeasurementForceData()
        {
            Measurement.ForceData = MeasuredValues.ToArray();
            OnPropertyChanged(nameof(MeasuredValues));
        }

        protected override bool OnSaveCanExecute()
        {
            return base.OnSaveCanExecute() && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosing()
        {
            if (HasChanges || _bleRecorderManager.IsCurrentlyMeasuring)
            {
                string message = _bleRecorderManager.IsCurrentlyMeasuring
                    ? "Measurement is currently running. Do you want to close this item and stop measurement?"
                    : "Do you want to discard all unsaved changes and close this item?";
                var result = await MessageDialogService.ShowOkCancelDialogAsync(message, "Closing tab");
                if (result == MessageDialogResult.Cancel) return false;
            }
            await StopMeasurement();
            return true;
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs message)
        {
            if (message.ViewModelName != nameof(TestSubjectDetailViewModel) ||
                message.Id != Measurement.TestSubjectId) return;

            if (await _measurementRepository.GetByIdAsync(Id) is null && Id > 0)
            {
                RaiseDetailClosedEvent();
                return;
            }

            await _measurementRepository.ReloadTestSubjectAsync(Measurement.TestSubject);
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs message)
        {
            if (message.ViewModelName == nameof(TestSubjectDetailViewModel) && message.Id == Measurement.TestSubjectId)
            {
                RaiseDetailClosedEvent();
            }
        }
    }
}
