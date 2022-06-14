using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.Wrapper;
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
            IMyodamManager myodamManager,
            IMeasurementRepository measurementRepository) : base(messenger, messageDialogService)
        {
            StartMeasurementCommand = new AsyncRelayCommand(StartMeasurement, () => true);
            StopMeasurementCommand = new AsyncRelayCommand(StopMeasurement, () => true);
            CleanRecordedDataCommand = new RelayCommand(CleanRecordedData, () => true);

            ForceValues = new ChartValues<double> { 2 };
            _myodamManager = myodamManager;
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
            _myodamManager.MyodamDevice.NewValueReceived += (sender, value) => { ForceValues.Add(value.Value); };
            await _myodamManager.MyodamDevice.StartMeasurement(new StimulationParameters(100, 50, 20));
        }


        public async Task StopMeasurement()
        {
            await _myodamManager.MyodamDevice.StopMeasurement();
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
