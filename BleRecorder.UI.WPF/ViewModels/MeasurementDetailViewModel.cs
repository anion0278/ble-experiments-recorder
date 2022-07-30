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
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.View.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Swordfish.NET.Collections.Auxiliary;
using WinRT;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IBleRecorderManager _bleRecorderManager;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IDateTimeService _dateTimeService;

        public ChartValues<double> ForceValues { get; set; } = new();

        [Required]
        [StringLength(30, MinimumLength = 1)]
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
            IBleRecorderManager bleRecorderManager,
            IMeasurementRepository measurementRepository,
            IDateTimeService dateTimeService) : base(messenger, messageDialogService)
        {
            _bleRecorderManager = bleRecorderManager;
            _measurementRepository = measurementRepository;
            _dateTimeService = dateTimeService;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => _bleRecorderManager.IsCurrentlyMeasuring);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed. Required due to the way ChartValues work

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        protected override void UnsubscribeOnClosing()
        {
            ForceValues.CollectionChanged -= OnForceValuesChanged;
            PropertyChanged -= OnPropertyChangedEventHandler;
            _bleRecorderManager.BleRecorderAvailabilityChanged -= OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;

            if (_bleRecorderManager.BleRecorderDevice != null)
            {
                _bleRecorderManager.BleRecorderDevice.NewValueReceived -= OnNewValueReceived;
                _bleRecorderManager.BleRecorderDevice.MeasurementFinished -= OnMeasurementFinished;
            }

            Messenger.Unregister<AfterDetailSavedEventArgs>(this);
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
            return _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        private void OnBleRecorderStatusChanged(object? o, EventArgs eventArgs)
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

            ForceValues.AddRange(Measurement.ForceData?.Select(v => v.Value) ?? Array.Empty<double>());
            Id = measurementId;

            PropertyChanged += OnPropertyChangedEventHandler;
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
                    "Measurement already contains data. Starting a new measurement will erase the existing data. Do you want to continue?",
                    "Delete measurement data?");
                if (result != MessageDialogResult.OK) return;
            }

            ForceValues.Clear();
            Date = _dateTimeService.Now;
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived += OnNewValueReceived; 
            _bleRecorderManager.BleRecorderDevice!.MeasurementFinished += OnMeasurementFinished; 
            await _bleRecorderManager.BleRecorderDevice.StartMeasurement();
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
            return base.OnSaveCanExecute() && !_bleRecorderManager.IsCurrentlyMeasuring;
        }

        protected override async Task<bool> UserAcknowledgedClosing()
        {
            if (HasChanges || _bleRecorderManager.IsCurrentlyMeasuring)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "You've made changes or measurement is currently running. Do you want to close this item and stop measurement?", "Question");
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
