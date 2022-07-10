using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IMyodamManager _myodamManager;
        private IMeasurementRepository _measurementRepository;
        private IDateTimeService _dateTimeService;

        public ChartValues<float> ForceValues { get; set; } = new();

        [Required]
        [MinLength(1, ErrorMessage = "{0} should contain at least {1} characters.")]
        [MaxLength(30, ErrorMessage = "{0} should contain maximum of {1} characters.")]
        public override string Title
        {
            get => Measurement.Title;
            set => Measurement.Title = value;
        }

        public string Notes
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
            IMeasurementRepository measurementRepository, 
            IDateTimeService dateTimeService) : base(messenger, messageDialogService)
        {
            _myodamManager = myodamManager;
            _measurementRepository = measurementRepository;
            _dateTimeService = dateTimeService;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => _myodamManager.IsCurrentlyMeasuring);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed
            PropertyChanged += OnPropertyChangedEventHandler; 

            _myodamManager.MyodamAvailabilityChanged += OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            //messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        protected override void UnsubscribeOnClosing()
        {
            ForceValues.CollectionChanged -= OnForceValuesChanged;
            PropertyChanged -= OnPropertyChangedEventHandler;
            _myodamManager.MyodamAvailabilityChanged -= OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;
            Messenger.Unregister<AfterDetailDeletedEventArgs>(this);
        }

        private void OnPropertyChangedEventHandler(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
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
            Measurement = measurementId > 0
                ? await _measurementRepository.GetByIdAsync(measurementId)
                : await CreateNewMeasurement((TestSubject)argsData);

            ForceValues.AddRange(Measurement.ForceData?.Select(v => v.Value) ?? Array.Empty<float>());
            Id = measurementId;
        }


        private async Task<Measurement> CreateNewMeasurement(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement
            {
                ForceData = new List<MeasuredValue>(),
                Notes = string.Empty
            };
            newMeasurement.TestSubject = (await _measurementRepository.GetTestSubjectById(correspondingTestSubject.Id))!;
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurement()
        {
            if (ForceValues.Count > 0)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "Measurement already contains data. Starting new measurement will erase the existing data. Do you want to continue?",
                    "Delete measurement data?");
                if (result != MessageDialogResult.OK) return;
            }

            ForceValues.Clear();
            Date = _dateTimeService.Now;
            _myodamManager.MyodamDevice!.NewValueReceived += (_, value) => { ForceValues.Add(value.Value); }; // TODO unsubscribe !!!
            await _myodamManager.MyodamDevice.StartMeasurement(new StimulationParameters(100, 50, StimulationPulseWidth.AvailableOptions[1], MeasurementType.MaximumContraction));
        }

        public async Task StopMeasurement()
        {
            if (_myodamManager.MyodamDevice != null) await _myodamManager.MyodamDevice.StopMeasurement();
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync($"Do you really want to delete the measurement {Measurement.Title}?", "Question");
            if (result == MessageDialogResult.OK)
            {
                _measurementRepository.Remove(Measurement);
                await _measurementRepository.SaveAsync();
                RaiseDetailDeletedEvent(Measurement.Id);
            }
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

        protected override async Task<bool> UserAcknowledgedClosing()
        {
            if (HasChanges || _myodamManager.IsCurrentlyMeasuring)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "You've made changes or measurement is currently running. Do you want to close this item and stop measurement?", "Question");
                if (result == MessageDialogResult.Cancel) return false;
            }
            await StopMeasurement();
            return true;
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel) && args.Id == Measurement.TestSubjectId)
            {
                RaiseDetailClosedEvent();
            }
        }
    }
}
