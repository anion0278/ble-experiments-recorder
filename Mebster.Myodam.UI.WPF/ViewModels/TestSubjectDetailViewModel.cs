using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Castle.Components.DictionaryAdapter;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Lookups;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.Wrapper;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class TestSubjectDetailViewModel : DetailViewModelBase, ITestSubjectDetailViewModel
    {
        private ITestSubjectRepository _testSubjectRepository;
        private readonly IMeasurementRepository _measurementsRepository;

        public ICommand RemoveMeasurementCommand { get; set; }

        public ICommand EditMeasurementCommand { get; set; }

        public ICommand AddMeasurementCommand { get; set; }

        private ObservableCollection<Measurement> _measurements;

        public ICollectionView Measurements { get; set; }


        public TestSubjectWrapper TestSubject { get; private set; }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public TestSubjectDetailViewModel() : base(null, null)
        { }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMeasurementRepository measurementsRepository,
            IMessenger eventAggregator,
            IMessageDialogService messageDialogService)
          : base(eventAggregator, messageDialogService)
        {
            _testSubjectRepository = testSubjectRepository;
            _measurementsRepository = measurementsRepository;

            AddMeasurementCommand = new RelayCommand(OnAddMeasurement);
            EditMeasurementCommand = new RelayCommand(OnEditMeasurement);
            RemoveMeasurementCommand = new RelayCommand(OnRemoveMeasurement, () => _measurements.Any());
        }

        private void OnRemoveMeasurement()
        {
            if (Measurements.CurrentItem != null)
                _measurements.Remove((Measurement)Measurements.CurrentItem);
        }

        private void OnEditMeasurement()
        {
            EventAggregator.Send(
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

            _measurements = new ObservableCollection<Measurement>(TestSubject.Measurements);
            Measurements = new CollectionViewSource() { Source = _measurements }.View;
        }

        private void InitializeTestSubject(TestSubject testSubject)
        {
            TestSubject = new TestSubjectWrapper(testSubject);
            TestSubject.PropertyChanged += (s, e) =>
          {
              // TODO REFACTORING !!!
              if (!HasChanges)
              {
                  HasChanges = _testSubjectRepository.HasChanges();
              }
              if (e.PropertyName == nameof(TestSubject.HasErrors))
              {
                  ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
              }
              if (e.PropertyName == nameof(testSubject.FirstName) || e.PropertyName == nameof(testSubject.LastName))
              {
                  SetTitle();
              }
          };
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
            if (TestSubject.Id == 0)
            {
                // trigger the validation
                TestSubject.FirstName = "";
            }
            SetTitle();

            var now = DateTime.Now;

            TestSubject.Measurements.Add(
                new Measurement()
                {
                    Description = "Fatigue test " + new Random().Next(1, 100),
                    ForceData = new List<MeasuredValue>()
                    {
                        new MeasuredValue(3.3f, now.AddSeconds(1)),
                        new MeasuredValue(4.3f, now.AddSeconds(2)),
                        new MeasuredValue(5.3f, now.AddSeconds(3)),
                        new MeasuredValue(3.3f, now.AddSeconds(4)),
                    }
                });
            TestSubject.Measurements.Add(new Measurement()
            {
                Description = "Max force test " + new Random().Next(1, 100),
                ForceData = new List<MeasuredValue>()
                    {
                        new MeasuredValue(33.3f, now.AddSeconds(1)),
                        new MeasuredValue(44.3f, now.AddSeconds(2)),
                        new MeasuredValue(55.3f, now.AddSeconds(3)),
                        new MeasuredValue(33.3f, now.AddSeconds(4)),
                    }
            });
        }

        private void SetTitle() // TODO utilize FOdy
        {
            Title = $"{TestSubject.FirstName} {TestSubject.LastName}";
        }

        protected override async void OnSaveExecute()
        {
            await SaveAsync(_testSubjectRepository.SaveAsync, // TODO rewrite explicitly, expand
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
