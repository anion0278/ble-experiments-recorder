using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.View.Services;
using BleRecorder.UI.WPF.Wrapper;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IBleRecorderManager _bleRecorderManager;
        private IMeasurementRepository _measurementRepository;

        // ChartValues<double> implements INotifyCollectionChanged, but it is too concrete
        public IList<double> ForceValues { get; set; } 

        public ObservableCollection<TestSubject> AddedTestSubjects { get; }

        public ObservableCollection<TestSubject> AvailableTestSubjects { get; }

        public bool IsCurrentlyMeasuring { get; set; }

        public TestSubject RelatedTestSubject { get; set; }

        public MeasurementWrapper Measurement { get; private set; }

        public ICommand StartMeasurementCommand { get; }
        public ICommand StopMeasurementCommand { get; }
        public ICommand CleanRecordedDataCommand { get; set; }


        public MeasurementDetailViewModel(IMessenger messenger,
            IMessageDialogService messageDialogService,
            IBleRecorderManager bleRecorderManager,
            IMeasurementRepository measurementRepository) : base(messenger, messageDialogService)
        {
            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, () => true);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => true);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues = new ChartValues<double> { 2 };
            _bleRecorderManager = bleRecorderManager;
            _measurementRepository = measurementRepository;
            //messenger.Register<AfterDetailSavedEventArgs>(this, (s,e) => AfterDetailSaved(e));
            //messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));

            AddedTestSubjects = new ObservableCollection<TestSubject>();
            AvailableTestSubjects = new ObservableCollection<TestSubject>();
        }

        private void CleanRecordedData()
        {
            ForceValues.Clear();
        }

        public override async Task LoadAsync(int measurementId)
        {
            var measurement = _measurementRepository.GetByIdAsync(measurementId);
            Id = measurementId;
        }

        public async Task StartMeasurement()
        {
            _bleRecorderManager.BleRecorderDevice.NewValueReceived += (sender, value) => { ForceValues.Add(value.Value); };
            await _bleRecorderManager.BleRecorderDevice.StartMeasurement(new StimulationParameters(100, 50, 20));
        }


        public async Task StopMeasurement()
        {
            await _bleRecorderManager.BleRecorderDevice.StopMeasurement();
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync($"Do you really want to delete the measurement {Measurement.Title}?", "Question");
            if (result == MessageDialogResult.OK)
            {
                //_measurementRepository.Remove(Measurement.Model);
                await _measurementRepository.SaveAsync();
                RaiseDetailDeletedEvent(Measurement.Id);
            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Measurement != null && !Measurement.HasErrors && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }


        //private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        //{
        //    if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
        //    {
        //        await _measurementRepository.ReloadTestSubjectAsync(args.Id);
        //        _allTestSubjects = await _measurementRepository.GetAllTestSubjectsAsync();
        //    }
        //}

        //private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        //{
        //    if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
        //    {
        //        _allTestSubjects = await _measurementRepository.GetAllTestSubjectsAsync();
        //    }
        //}
    }
}
