using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.Wrappers;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private readonly IMyodamManager _myodamManager;
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
            IMyodamManager myodamManager,
            IMeasurementRepository measurementRepository) : base(messenger, messageDialogService)
        {
            _myodamManager = myodamManager;
            _measurementRepository = measurementRepository;

            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, StartMeasurementCanExecute);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => _myodamManager.IsCurrentlyMeasuring);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues = new ChartValues<double>();    
            _availableTestSubjects = new ObservableCollection<TestSubject>();
            AvailableTestSubjects = CollectionViewSource.GetDefaultView(_availableTestSubjects);

            _myodamManager.MyodamAvailabilityChanged += OnMyodamStatusChanged;
            _myodamManager.MeasurementStatusChanged += OnMeasuremetStatusChanged;

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
            return _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected && !_myodamManager.IsCurrentlyMeasuring;
        }

        private void OnMyodamStatusChanged(object? o, EventArgs eventArgs)
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
            _myodamManager.MyodamDevice!.NewValueReceived += (sender, value) => { ForceValues.Add(value.Value); };
            await _myodamManager.MyodamDevice.StartMeasurement(new StimulationParameters(100, 50, 20));
        }

        public async Task StopMeasurement()
        {
            await _myodamManager.MyodamDevice?.StopMeasurement();
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
