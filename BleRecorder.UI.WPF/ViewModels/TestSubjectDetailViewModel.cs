using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Castle.Components.DictionaryAdapter;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Lookups;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.View.Services;
using BleRecorder.UI.WPF.Wrapper;
using Prism.Commands;
using Prism.Events;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class TestSubjectDetailViewModel : DetailViewModelBase, ITestSubjectDetailViewModel
    {
        private ITestSubjectRepository _testSubjectRepository;
        private readonly IMeasurementRepository _measurementsRepository;
        private TestSubjectWrapper _testSubject;

        public ICommand RemoveMeasurementCommand { get; set; }

        public ICommand EditMeasurementCommand { get; set; }

        public ICommand AddMeasurementCommand { get; set; }


        private BindingList<Measurement> _measurements = new();

        public ICollectionView Measurements { get; set; }


        public TestSubjectWrapper TestSubject
        {
            get { return _testSubject; }
            private set
            {
                _testSubject = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public TestSubjectDetailViewModel() : base(null, null)
        { }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMeasurementRepository measurementsRepository,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService)
          : base(eventAggregator, messageDialogService)
        {
            _testSubjectRepository = testSubjectRepository;
            _measurementsRepository = measurementsRepository;

            Measurements = new CollectionViewSource() { Source = _measurements }.View;

            AddMeasurementCommand = new DehandateCommand(OnAddMeasurement);
            EditMeasurementCommand = new DehandateCommand(OnEditMeasurement);
            RemoveMeasurementCommand = new DehandateCommand(OnRemoveMeasurement, () => _measurements.Any());

            //eventAggregator.GetEvent<AfterCollectionSavedEvent>()
            // .Subscribe(AfterCollectionSaved);
        }

        private void OnRemoveMeasurement()
        {
            if (Measurements.CurrentItem != null)
                _measurements.Remove((Measurement)Measurements.CurrentItem);
        }

        private void OnEditMeasurement()
        {
            EventAggregator.GetEvent<OpenDetailViewEvent>().Publish(
                    new OpenDetailViewEventArgs
                    {
                        Id = -1,
                        ViewModelName = nameof(MeasurementDetailViewModel)
                    });
        }

        private void OnAddMeasurement()
        {
            throw new NotImplementedException();
        }

        public override async Task LoadAsync(int measurementId)
        {
            var testSubject = measurementId > 0
              ? await _testSubjectRepository.GetByIdAsync(measurementId)
              : CreateNewTestSubject();

            Id = measurementId;

            InitializeTestSubject(testSubject);

            //Measurements = _measurementsRepository.GetAllTestSubjectsAsync()
        }

        private void InitializeTestSubject(TestSubject testSubject)
        {
            _testSubject = new TestSubjectWrapper(testSubject);
            _testSubject.PropertyChanged += (s, e) =>
          {
              // TODO REFACTORING !!!
              if (!HasChanges)
              {
                  HasChanges = _testSubjectRepository.HasChanges();
              }
              if (e.PropertyName == nameof(_testSubject.HasErrors))
              {
                  ((DehandateCommand)SaveCommand).RaiseCanExecuteChanged();
              }
              if (e.PropertyName == nameof(testSubject.FirstName) || e.PropertyName == nameof(testSubject.LastName))
              {
                  SetTitle();
              }
          };
            ((DehandateCommand)SaveCommand).RaiseCanExecuteChanged();
            if (_testSubject.Id == 0)
            {
                // trigger the validation
                _testSubject.FirstName = "";
            }
            SetTitle();

            var now = DateTime.Now;

            _measurements.Add(
                new Measurement()
                {
                    Description = "Intermittent test 1",
                    ForceData = new List<MeasuredValue>()
                    {
                        new MeasuredValue(3.3f, now.AddSeconds(1)),
                        new MeasuredValue(4.3f, now.AddSeconds(2)),
                        new MeasuredValue(5.3f, now.AddSeconds(3)),
                        new MeasuredValue(3.3f, now.AddSeconds(4)),
                    }
                });
            _measurements.Add(new Measurement()
            {
                Description = "Max force test 2",
                ForceData = new List<MeasuredValue>()
                    {
                        new MeasuredValue(3.3f, now.AddSeconds(1)),
                        new MeasuredValue(4.3f, now.AddSeconds(2)),
                        new MeasuredValue(5.3f, now.AddSeconds(3)),
                        new MeasuredValue(3.3f, now.AddSeconds(4)),
                    }
            });
        }

        private void SetTitle() // TODO utilize FOdy
        {
            Title = $"{TestSubject.FirstName} {TestSubject.LastName}";
        }

        protected override async void OnSaveExecute()
        {
            await SaveAsync(_testSubjectRepository.SaveAsync,
              () =>
              {
                  HasChanges = _testSubjectRepository.HasChanges();
                  Id = TestSubject.Id;
                  RaiseDetailSavedEvent(TestSubject.Id, $"{TestSubject.FirstName} {TestSubject.LastName}");
              });
        }

        protected override bool OnSaveCanExecute()
        {
            return TestSubject != null
              && !TestSubject.HasErrors
              && HasChanges;
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the test Subject {TestSubject.FirstName} {TestSubject.LastName}?", "Confirmation is required");

            if (result == MessageDialogResult.OK)
            {
                _testSubjectRepository.Remove(TestSubject.Model);
                await _testSubjectRepository.SaveAsync();
                RaiseDetailDeletedEvent(TestSubject.Id);
            }
        }

        private TestSubject CreateNewTestSubject()
        {
            var testSubject = new TestSubject();
            _testSubjectRepository.Add(testSubject);
            return testSubject;
        }
    }
}
