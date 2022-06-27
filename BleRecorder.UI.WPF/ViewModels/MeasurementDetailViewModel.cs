﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveCharts;
using BleRecorder.Business.Device;
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

        // ChartValues<double> implements INotifyCollectionChanged, but it is too concrete
        public IList<float> ForceValues { get; set; }

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

        public DateTimeOffset Date
        {
            get => Measurement.Date;
            set => Measurement.Date = value;
        }

        //public TestSubject TestSubject
        //{
        //    get => Measurement.TestSubject;
        //    set => Measurement.TestSubject = value;
        //}

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
            IMeasurementRepository measurementRepository) : base(messenger, messageDialogService)
        {
            _bleRecorderManager = bleRecorderManager;
            _measurementRepository = measurementRepository;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => _bleRecorderManager.IsCurrentlyMeasuring);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues = new ChartValues<float>();

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasuremetStatusChanged;

            PropertyChanged += (_, _) => { HasChanges = _measurementRepository.HasChanges(); };

            //messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            //messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        private void OnMeasuremetStatusChanged(object? sender, EventArgs e)
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
                : CreateNewMeasurement((TestSubject)argsData);
            
            ForceValues.AddRange(Measurement.ForceData?.Select(v => v.Value) ?? Array.Empty<float>());

            Id = measurementId;
        }


        private Measurement CreateNewMeasurement(TestSubject correspondingTestSubject)
        {
            var newMeasurement = new Measurement();
            newMeasurement.ForceData = new List<MeasuredValue>();
            _measurementRepository.StartTrackingTestSubject(correspondingTestSubject);
            newMeasurement.TestSubject = correspondingTestSubject;
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurement()
        {
            Date = DateTimeOffset.Now;
            _bleRecorderManager.BleRecorderDevice!.NewValueReceived += (sender, value) => { ForceValues.Add(value.Value); };
            await _bleRecorderManager.BleRecorderDevice.StartMeasurement(new StimulationParameters(100, 50, 20));
        }

        public async Task StopMeasurement()
        {
            await _bleRecorderManager.BleRecorderDevice?.StopMeasurement();
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync($"Do you really want to delete the measurement {Measurement.Title}?", "Question");
            if (result == MessageDialogResult.OK)
            {
                //_measurementRepository.Remove(_measurement.Model);
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
                .Select((val, index) => new MeasuredValue((float)val, DateTime.Now.AddSeconds(index)))
                .ToArray();
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
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
