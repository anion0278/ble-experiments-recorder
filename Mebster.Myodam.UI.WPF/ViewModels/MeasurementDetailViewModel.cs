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
        private readonly IMeasurementFactory _measurementFactory;
        private IMeasurementStrategy _measurementStrategy;
        private MeasurementBase _model;

        public ChartValues<MeasuredValue> MeasuredValues { get; set; } = new();

        public MeasurementBase Model
        {
            get => _model;
            private set
            {
                if (_model == value) return;

                _model = value;
                _measurementStrategy = _measurementFactory.GetMeasurementStrategy(_model, MeasuredValues);
            }
        }

        public MeasurementType Type
        {
            get => Model.Type;
            set
            {
                if (value == Model.Type) return;

                _measurementRepository.Remove(Model);
                Model = _measurementFactory.ChangeMeasurementType(value, Model, _mapper);
                _measurementRepository.Add(Model);
            }
        }

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

        public string TestSubjectName => Model.TestSubject.FullName;

        public StimulationParametersViewModel StimulationParametersVm { get; private set; }

        public MechanismParametersViewModel MechanismParametersVm { get; private set; }

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
            IMeasurementFactory measurementFactory) : base(messenger, dialogService, myodamManager)
        {
            _myodamManager = myodamManager;
            _mapper = mapper;
            _measurementRepository = measurementRepository;
            _measurementFactory = measurementFactory;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync, () =>
            {
                Debug.Print(_measurementStrategy.CanStartMeasurement.ToString());
                return _measurementStrategy.CanStartMeasurement;
            });
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurementAsync, () => _measurementStrategy.CanStopMeasurement);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedDataAsync, () => !_measurementStrategy.IsCurrentlyMeasuring);

            MeasuredValues.CollectionChanged += OnContractionValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _myodamManager.MyodamAvailabilityChanged += OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSavedAsync(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        protected override bool OnDeleteCanExecute()
        {
            return !_measurementStrategy.CanStopMeasurement;
        }

        protected override void UnsubscribeOnClosing()
        {
            _measurementStrategy.Dispose();

            MechanismParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            StimulationParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            PropertyChanged -= OnPropertyChangedEventHandler;

            MeasuredValues.CollectionChanged -= OnContractionValuesChanged;
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

        private void OnContractionValuesChanged(object? o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RunInViewContext(() => OnPropertyChanged(nameof(MeasuredValues)));
        }

        private void OnMeasurementStatusChanged(object? sender, EventArgs e)
        {
            RunInViewContext(() =>
            {
                StartMeasurementCommand.NotifyCanExecuteChanged();
                StopMeasurementCommand.NotifyCanExecuteChanged();
                CleanRecordedDataCommand.NotifyCanExecuteChanged();
                SaveCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                if (_measurementStrategy.IsCurrentlyMeasuring) return;

                StopMeasurementCommand.NotifyCanExecuteChanged();
                if (_myodamManager.MyodamDevice is not null && _myodamManager.MyodamDevice.IsConnected) return;

                MeasuredValues.Clear();
                DialogService.ShowInfoDialogAsync("Measurement was interrupted due to device disconnection! Measured data were erased.");
            });
        }

        private void OnMyodamStatusChanged(object? o, EventArgs eventArgs)
        {
            RunInViewContext(() => StartMeasurementCommand.NotifyCanExecuteChanged());
        }

        private async void CleanRecordedDataAsync()
        {
            if (MeasuredValues.Count <= 0) return;

            var result = await DialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to remove measurement data?",
                "Delete data?");
            if (result == MessageDialogResult.OK)
            {
                _measurementStrategy.ClearRecordedData();
            }
            NotifyMeasurementDataChanged();
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

            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
        }

        private async Task<MeasurementBase> CreateNewMeasurementAsync(TestSubject correspondingTestSubject)
        {
            var newMeasurement = _measurementFactory.CreateNewEmptyMeasurement(
                (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!);

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

            await _measurementStrategy.StartMeasurement();
        }

        public async Task StopMeasurementAsync()
        {
            if (await _measurementStrategy.StopMeasurement())
            {
                await DialogService.ShowInfoDialogAsync("Measured values were erased because measurement was interrupted.");
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

            NotifyMeasurementDataChanged();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Model.Id;
            RaiseDetailSavedEvent(Model.Id, Model.Title);
        }

        private void NotifyMeasurementDataChanged()
        {
            OnPropertyChanged(nameof(MeasuredValues));
        }

        protected override bool OnSaveCanExecute()
        {
            return base.OnSaveCanExecute() && !_measurementStrategy.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosingAsync()
        {
            if (HasChanges || _measurementStrategy.IsCurrentlyMeasuring)
            {
                string message = _measurementStrategy.IsCurrentlyMeasuring
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
            OnPropertyChanged(nameof(TestSubjectName));
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
