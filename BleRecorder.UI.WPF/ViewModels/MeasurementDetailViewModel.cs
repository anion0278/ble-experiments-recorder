using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using BleRecorder.UI.WPF.Wrappers;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IBleRecorderManager _bleRecorderManager;
        private IMeasurementRepository _measurementRepository;
        private IDateTimeService _dateTimeService;

        public ChartValues<float> ForceValues { get; set; } = new();

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

            ForceValues.CollectionChanged += OnForceValuesChanged; // letting ComboBox.IsDisabled know that collection changed
            PropertyChanged += OnPropertyChangedEventHandler; // TODO try use Context.ChangeTracker.StateChanged

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasurementStatusChanged;

            //messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            //messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
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
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived += (_, value) => { ForceValues.Add(value.Value); }; // TODO unsubscribe !!!
            await _bleRecorderManager.BleRecorderDevice.StartMeasurement(new StimulationParameters(100, 50, StimulationPulseWidth.AvailableOptions[1], MeasurementType.MaximumContraction));
        }

        public async Task StopMeasurement()
        {
            if (_bleRecorderManager.BleRecorderDevice != null) await _bleRecorderManager.BleRecorderDevice.StopMeasurement();
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

        protected override bool OnSaveCanExecute()
        {
            return Measurement != null && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            Measurement.ForceData = ForceValues
                .Select((val, index) => new MeasuredValue((float)val, TimeSpan.FromMilliseconds(index)))
                .ToArray();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }

        protected override async void OnCloseDetailViewExecute()
        {
            if (HasChanges || _bleRecorderManager.IsCurrentlyMeasuring)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                    "You've made changes or measurement is currently running. Do you want to close this item and stop measurement?", "Question");
                if (result == MessageDialogResult.Cancel) return;
            }

            ForceValues.CollectionChanged -= OnForceValuesChanged;
            PropertyChanged -= OnPropertyChangedEventHandler;

            await StopMeasurement();
            Messenger.Send(new AfterDetailClosedEventArgs
            {
                Id = Id,
                ViewModelName = GetType().Name
            });
        }


        //private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        //{
        //    if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
        //    {
        //        //await _measurementRepository.ReloadTestSubjectAsync(args.Id);
        //    }
        //}


        // TODO handle deletion of TestSubjects even when Meas is not saved !
        //private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        //{
        //    if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
        //    {


        //    }
        //}
    }
}
