﻿using System;
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
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore;
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

        public override string Title => string.IsNullOrWhiteSpace(Model.Title) ? "(New measurement)" : Model.Title;

        [Required]
        [StringLength(30, MinimumLength = 1)]
        [Display(Name = "Short description")]
        [AlsoNotifyFor(nameof(Title))]
        public string MeasurementDescription
        {
            get => Model.Title;
            set => Model.Title = value;
        }

        public string? Notes
        {
            get => Model.Notes;
            set => Model.Notes = value;
        }

        public DateTimeOffset? Date
        {
            get => Model.Date;
            set => Model.Date = value;
        }

        public MeasurementType Type
        {
            get => Model.Type;
            set => Model.Type = value;
        }

        public PositionDuringMeasurement Position
        {
            get => Model.PositionDuringMeasurement;
            set => Model.PositionDuringMeasurement = value;
        }

        public MeasurementSite Site
        {
            get => Model.SiteDuringMeasurement;
            set => Model.SiteDuringMeasurement = value;
        }

        public StimulationParametersViewModel StimulationParametersVm { get; private set; }

        public MechanismParametersViewModel MechanismParametersVm { get; private set; }

        public Measurement Model { get; private set; }

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

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurementAsync, StopMeasurementCanExecute);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedDataAsync, () => !_bleRecorderManager.IsCurrentlyMeasuring);

            MeasuredValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSavedAsync(e));
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
            if (_bleRecorderManager.BleRecorderDevice != null) _bleRecorderManager.BleRecorderDevice.NewValueReceived -= OnNewValueReceived;

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
            MechanismParametersVm.CopyAdjustmentValuesTo(Model.AdjustmentsDuringMeasurement!);

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
                SaveCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                if (_bleRecorderManager.IsCurrentlyMeasuring) return;

                StopMeasurementCommand.NotifyCanExecuteChanged();
                if (_bleRecorderManager.BleRecorderDevice is not null && _bleRecorderManager.BleRecorderDevice.IsConnected) return;

                MeasuredValues.Clear();
                DialogService.ShowInfoDialogAsync("Measurement interrupted due to device disconnection! Measured data were erased.");
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

        private async void CleanRecordedDataAsync()
        {
            if (MeasuredValues.Count <= 0) return;

            var result = await DialogService.ShowOkCancelDialogAsync(
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
            Model = (measurementId > 0
                ? await _measurementRepository.GetByIdAsync(measurementId)
                : await CreateNewMeasurementAsync(ts))!;

            Model.ParametersDuringMeasurement ??= (StimulationParameters)ts.CustomizedParameters.Clone();
            StimulationParametersVm = new StimulationParametersViewModel(Model.ParametersDuringMeasurement);
            StimulationParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            Model.AdjustmentsDuringMeasurement ??= (DeviceMechanicalAdjustments)ts.CustomizedAdjustments.Clone();
            MechanismParametersVm = new MechanismParametersViewModel(new MechanismParameters(Model.AdjustmentsDuringMeasurement), _mapper);
            MechanismParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            MeasuredValues.AddRange(Model.ForceData);
            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
        }


        private async Task<Measurement> CreateNewMeasurementAsync(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement
            {
                ForceData = new List<MeasuredValue>(),
                TestSubject = (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!
            };
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurementAsync()
        {
            if (!_bleRecorderManager.Calibration.IsValid())
            {
                await DialogService.ShowInfoDialogAsync("Device calibration is invalid. Measurement is disabled.");
                return;
            }

            var result = await DialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to start measurement with current parameters listed in this page (they may differ from user-specific parameter settings)?",
                "Start measurement?");
            if (result != MessageDialogResult.OK) return;

            if (MeasuredValues.Count > 0)
            {
                result = await DialogService.ShowOkCancelDialogAsync(
                    "Measurement already contains data. Starting a new measurement will erase the existing data. Do you want to continue?",
                    "Delete measurement data?");
                if (result != MessageDialogResult.OK) return;
            }

            MeasuredValues.Clear();
            Date = _dateTimeService.Now;
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived += OnNewValueReceived;
            await _bleRecorderManager.BleRecorderDevice.StartMeasurementAsync(Model.ParametersDuringMeasurement!, Type);
        }

        private void OnNewValueReceived(object? _, MeasuredValue sensorMeasuredValue)
        {
            var forceValue = sensorMeasuredValue with
            {
                Value = sensorMeasuredValue.Value / _bleRecorderManager.Calibration.NoLoadSensorValue
            };
            MeasuredValues.Add(forceValue);
        }

        public async Task StopMeasurementAsync()
        {
            if (_bleRecorderManager.BleRecorderDevice != null)
            {
                await _bleRecorderManager.BleRecorderDevice.StopMeasurementAsync();
                _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived;
                MeasuredValues.Clear();

                if (_bleRecorderManager.IsCurrentlyMeasuring)
                {
                    await DialogService.ShowInfoDialogAsync("Measured values were erased because measurement was interrupted.");
                }
            }
        }

        protected override async void OnDeleteExecuteAsync() // TODO into base class, since TS OnDeleteExecute is almost same. Abstract a single main repository
        {
            if (Id < 0)
            {
                OnCloseDetailViewExecuteAsync();
                return;
            }

            var result = await DialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the measurement {Model.Title}?", "Question");

            if (result != MessageDialogResult.OK) return;

            _measurementRepository.Remove(Model);
            await _measurementRepository.SaveAsync();
            RaiseDetailDeletedEvent(Model.Id);
        }

        protected override async void OnSaveExecuteAsync()
        {
            await _measurementRepository.ReloadTestSubjectAsync(Model.TestSubject);
            var currentMeasurements = Model.TestSubject.Measurements;
            if (currentMeasurements.Any(ts => ts.Title == Model.Title && ts.Id != Model.Id))
            {
                await DialogService.ShowInfoDialogAsync($"A measurement with name '{Model.Title}' already exists. Please change the name.");
                return;
            }

            UpdateMeasurementForceData();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Model.Id;
            RaiseDetailSavedEvent(Model.Id, Model.Title);
        }

        private void UpdateMeasurementForceData()
        {
            Model.ForceData = MeasuredValues.ToArray();
            OnPropertyChanged(nameof(MeasuredValues));
        }

        protected override bool OnSaveCanExecute()
        {
            return base.OnSaveCanExecute() && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosingAsync()
        {
            if (HasChanges || _bleRecorderManager.IsCurrentlyMeasuring)
            {
                string message = _bleRecorderManager.IsCurrentlyMeasuring
                    ? "Measurement is currently running. Do you want to close this item and stop measurement?"
                    : "Do you want to discard all unsaved changes and close this item?";
                var result = await DialogService.ShowOkCancelDialogAsync(message, "Closing tab");
                if (result == MessageDialogResult.Cancel) return false;
            }
            await StopMeasurementAsync();
            return true;
        }

        private async void AfterDetailSavedAsync(AfterDetailSavedEventArgs message)
        {
            if (message.ViewModelName != nameof(TestSubjectDetailViewModel) ||
                message.Id != Model.TestSubjectId) return;

            if (await _measurementRepository.GetByIdAsync(Id) is null && Id > 0)
            {
                RaiseDetailClosedEvent();
                return;
            }

            await _measurementRepository.ReloadTestSubjectAsync(Model.TestSubject);
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs message)
        {
            if (message.ViewModelName == nameof(TestSubjectDetailViewModel) && message.Id == Model.TestSubjectId)
            {
                RaiseDetailClosedEvent();
            }
        }
    }
}
