using System;
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

namespace BleRecorder.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IBleRecorderManager _bleRecorderManager;
        private IMeasurementRepository _measurementRepository;

        // ChartValues<double> implements INotifyCollectionChanged, but it is too concrete
        public IList<double> ForceValues { get; set; } 

        private ObservableCollection<TestSubject> _availableTestSubjects { get; }

        public ICollectionView AvailableTestSubjects { get; }
        
        public MeasurementWrapper Measurement { get; private set; }

        public IRelayCommand StartMeasurementCommand { get; }
        public IRelayCommand StopMeasurementCommand { get; }
        public IRelayCommand CleanRecordedDataCommand { get; set; }


        /// <summary>
        /// Design-time ctor    
        /// </summary>
        public MeasurementDetailViewModel():base(null!, null!)
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

            ForceValues = new ChartValues<double>();    
            _availableTestSubjects = new ObservableCollection<TestSubject>();
            AvailableTestSubjects = CollectionViewSource.GetDefaultView(_availableTestSubjects);

            _bleRecorderManager.BleRecorderAvailabilityChanged += OnBleRecorderStatusChanged;
            _bleRecorderManager.MeasurementStatusChanged += OnMeasuremetStatusChanged;

            messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
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

        private void CleanRecordedData()
        {
            ForceValues.Clear();
        }

        public override async Task LoadAsync(int measurementId)
        {
            var measurement = measurementId > 0
                ? await _measurementRepository.GetByIdAsync(measurementId)
                : CreateNewMeasurement();

            Id = measurementId;

            Measurement = new MeasurementWrapper(measurement);
            Measurement.PropertyChanged += (_,_) =>
            {
                HasChanges = _measurementRepository.HasChanges();
            };

            await ReloadTestSubjects();

            ////_measurement = new MeasurementWrapper(await _measurementRepository.GetByIdAsync(measurementId));
            //if (measurementId == -1)
            //{
            //    _measurement = new MeasurementWrapper(new Measurement());
            //}
            //
            //Id = measurementId;
        }

        private Measurement CreateNewMeasurement()
        {
            var newMeasurement = new Measurement();
            newMeasurement.ForceData = new List<MeasuredValue>();
            _measurementRepository.Add(newMeasurement);
            return newMeasurement;
        }

        public async Task StartMeasurement()
        {
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
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }


        private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
            {
                //await _measurementRepository.ReloadTestSubjectAsync(args.Id);
                await ReloadTestSubjects();
            }
        }

        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
            {
                await ReloadTestSubjects();
            }
        }

        private async Task ReloadTestSubjects()
        {
            _availableTestSubjects.Clear();
            foreach (var testSubject in (await _measurementRepository.GetAllTestSubjectsAsync()))
                _availableTestSubjects.Add(testSubject);
        }
    }
}
