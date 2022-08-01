using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using AutoMapper;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Views.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
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

        public ChartValues<double> ForceValues { get; set; } = new();

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
        public MeasurementDetailViewModel() : base(null!, null!)
        {
        }

        public MeasurementDetailViewModel(IMessenger messenger,
            IMessageDialogService messageDialogService,
            IMyodamManager myodamManager,
            IMapper mapper,
            IMeasurementRepository measurementRepository,
            IDateTimeService dateTimeService) : base(messenger, messageDialogService)
        {
            _myodamManager = myodamManager;
            _mapper = mapper;
            _measurementRepository = measurementRepository;
            _dateTimeService = dateTimeService;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => _myodamManager.IsCurrentlyMeasuring);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => !_myodamManager.IsCurrentlyMeasuring);

            ForceValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _myodamManager.MyodamAvailabilityChanged += OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        protected override void UnsubscribeOnClosing()
        {
            MechanismParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            StimulationParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            PropertyChanged -= OnPropertyChangedEventHandler;

            ForceValues.CollectionChanged -= OnForceValuesChanged;
            _myodamManager.MyodamAvailabilityChanged -= OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;

            if (_myodamManager.MyodamDevice != null)
            {
                _myodamManager.MyodamDevice.NewValueReceived -= OnNewValueReceived;
                _myodamManager.MyodamDevice.MeasurementFinished -= OnMeasurementFinished;
            }

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
            OnPropertyChanged(nameof(ForceValues));
        }

        private void OnMeasurementStatusChanged(object? sender, EventArgs e)
        {
            StartMeasurementCommand.NotifyCanExecuteChanged();
            StopMeasurementCommand.NotifyCanExecuteChanged();
            CleanRecordedDataCommand.NotifyCanExecuteChanged();
        }

        private bool StartMeasurementCanExecute()
        {
            return _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected && !_myodamManager.IsCurrentlyMeasuring;
        }

        private void OnMyodamStatusChanged(object? o, EventArgs eventArgs)
        {
            ViewSynchronizationContext.Send(_ => StartMeasurementCommand.NotifyCanExecuteChanged(), null);
        }

        private async void CleanRecordedData()
        {
            if (ForceValues.Count <= 0) return;

            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to remove measurement data?",
                "Delete data?");
            if (result == MessageDialogResult.OK)
            {
                ForceValues.Clear();
            }
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

            ForceValues.AddRange(Measurement.ForceData?.Select(v => v.Value) ?? Array.Empty<double>());
            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
        }


        private async Task<Measurement> CreateNewMeasurement(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement
            {
                ForceData = new List<MeasuredValue>(),
            };
            newMeasurement.TestSubject = (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!;
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurement()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                "Are you sure you want to start measurement with current parameters listed in this page (they may differ from user-specific parameter settings)?",
                "Start measurement?");
            if (result != MessageDialogResult.OK) return;

            if (ForceValues.Count > 0)
            {
                result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "Measurement already contains data. Starting a new measurement will erase the existing data. Do you want to continue?",
                    "Delete measurement data?");
                if (result != MessageDialogResult.OK) return;
            }

            ForceValues.Clear();
            Date = _dateTimeService.Now;
            _myodamManager.MyodamDevice!.NewValueReceived += OnNewValueReceived;
            _myodamManager.MyodamDevice!.MeasurementFinished += OnMeasurementFinished;
            await _myodamManager.MyodamDevice.StartMeasurement();
        }

        private void OnMeasurementFinished(object? _, EventArgs value)
        {
            MessageDialogService.ShowInfoDialogAsync("Measurement finished.");
        }

        private void OnNewValueReceived(object? _, MeasuredValue value)
        {
            ForceValues.Add(value.Value);
        }

        public async Task StopMeasurement()
        {
            if (_myodamManager.MyodamDevice != null)
            {
                await _myodamManager.MyodamDevice.StopMeasurement();
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
            Measurement.ForceData = ForceValues
                .Select((val, index) => new MeasuredValue(val, TimeSpan.FromMilliseconds(index)))
                .ToArray();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }

        protected override bool OnSaveCanExecute()
        {
            return base.OnSaveCanExecute() && !_myodamManager.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosing()
        {
            if (HasChanges || _myodamManager.IsCurrentlyMeasuring)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "You made changes or measurement is currently running. Do you want to close this item and stop measurement?", "Question");
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
