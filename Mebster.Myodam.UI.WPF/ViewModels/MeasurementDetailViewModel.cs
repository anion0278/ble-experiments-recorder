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
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore;
using PropertyChanged;
using Swordfish.NET.Collections.Auxiliary;
using WinRT;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IMyodamManager _myodamManager;
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
            IMyodamManager myodamManager,
            IMapper mapper,
            IMeasurementRepository measurementRepository,
            IDateTimeService dateTimeService) : base(messenger, dialogService, myodamManager)
        {
            _myodamManager = myodamManager;
            _mapper = mapper;
            _measurementRepository = measurementRepository;
            _dateTimeService = dateTimeService;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurementAsync, StopMeasurementCanExecute);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedDataAsync, () => !_myodamManager.IsCurrentlyMeasuring);

            MeasuredValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _myodamManager.MyodamAvailabilityChanged += OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSavedAsync(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private bool StopMeasurementCanExecute()
        {
            return _myodamManager.IsCurrentlyMeasuring && !_myodamManager.MyodamDevice!.IsCalibrating;
        }

        protected override bool OnDeleteCanExecute()
        {
            return !StopMeasurementCanExecute();
        }

        protected override void UnsubscribeOnClosing()
        {
            if (_myodamManager.MyodamDevice != null) _myodamManager.MyodamDevice.NewValueReceived -= OnNewValueReceived;

            MechanismParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            StimulationParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            PropertyChanged -= OnPropertyChangedEventHandler;

            MeasuredValues.CollectionChanged -= OnForceValuesChanged;
            _myodamManager.MyodamAvailabilityChanged -= OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;

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
            if (!_myodamManager.IsCurrentlyMeasuring && _myodamManager.MyodamDevice != null) //_myodamManager.MyodamDevice != null
            {
                _myodamManager.MyodamDevice.NewValueReceived -= OnNewValueReceived;
            }

            // TODO put Send method into VMBase 
            ViewSynchronizationContext.Send(_ =>
            {
                StartMeasurementCommand.NotifyCanExecuteChanged();
                StopMeasurementCommand.NotifyCanExecuteChanged();
                CleanRecordedDataCommand.NotifyCanExecuteChanged();
                SaveCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                if (_myodamManager.IsCurrentlyMeasuring) return;

                StopMeasurementCommand.NotifyCanExecuteChanged();
                if (_myodamManager.MyodamDevice is not null && _myodamManager.MyodamDevice.IsConnected) return;

                MeasuredValues.Clear();
                DialogService.ShowInfoDialogAsync("Measurement was interrupted due to device disconnection! Measured data were erased.");
            }, null);
        }

        private bool StartMeasurementCanExecute()
        {
            return _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected && !_myodamManager.IsCurrentlyMeasuring;
        }

        private void OnMyodamStatusChanged(object? o, EventArgs eventArgs)
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

            MeasuredValues.AddRange(Model.ContractionLoadData);
            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
        }


        private async Task<Measurement> CreateNewMeasurementAsync(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement
            {
                ContractionLoadData = new List<MeasuredValue>(),
                TestSubject = (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!
            };
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurementAsync()
        {
            if (!_myodamManager.Calibration.IsValid())
            {
                await DialogService.ShowInfoDialogAsync("Device calibration is invalid. Measurement is disabled.");
                return;
            }
            _myodamManager.Calibration.UpdateCalibration();


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
            _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
            _myodamManager.MyodamDevice!.NewValueReceived += OnNewValueReceived;
            await _myodamManager.MyodamDevice.StartMeasurementAsync(Model.ParametersDuringMeasurement!, Type);
            Date = _dateTimeService.Now;
        }

        private void OnNewValueReceived(object? _, MeasuredValue sensorMeasuredValue)
        {
            var forceValue = sensorMeasuredValue with
            {
                ContractionValue = _myodamManager.Calibration.CalculateLoadValue(sensorMeasuredValue.ContractionValue)
            };
            MeasuredValues.Add(forceValue);
        }

        public async Task StopMeasurementAsync()
        {
            if (_myodamManager.MyodamDevice != null)
            {
                await _myodamManager.MyodamDevice.StopMeasurementAsync();
                _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived;
                MeasuredValues.Clear();

                if (_myodamManager.IsCurrentlyMeasuring)
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
            Model.ContractionLoadData = MeasuredValues.ToArray();
            OnPropertyChanged(nameof(MeasuredValues));
        }

        protected override bool OnSaveCanExecute()
        {
            return base.OnSaveCanExecute() && !_myodamManager.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosingAsync()
        {
            if (HasChanges || _myodamManager.IsCurrentlyMeasuring)
            {
                string message = _myodamManager.IsCurrentlyMeasuring
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
