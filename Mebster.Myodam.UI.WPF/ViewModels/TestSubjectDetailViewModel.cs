﻿using System;
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
using Mebster.Myodam.UI.WPF.Wrappers;
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
            IMessenger messenger,
            IMessageDialogService messageDialogService)
          : base(messenger, messageDialogService)
        {
            _testSubjectRepository = testSubjectRepository;
            _measurementsRepository = measurementsRepository;

            AddMeasurementCommand = new RelayCommand(OnAddMeasurement);
            EditMeasurementCommand = new RelayCommand(OnEditMeasurement, () => Measurements!.CurrentItem != null);
            RemoveMeasurementCommand = new RelayCommand(OnRemoveMeasurement, () => Measurements!.CurrentItem != null);

            messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs message)
        {
            if (message.ViewModelName == nameof(MeasurementDetailViewModel))
            {
                //await LoadAsync(TestSubject.Id, null); // TODO solve
            }
        }

        private void OnAddMeasurement()
        {
            Messenger.Send(new OpenDetailViewEventArgs()
            {
                Id = -1,
                ViewModelName = nameof(MeasurementDetailViewModel),
                Data = TestSubject.Model
            });
        }

        private void OnEditMeasurement()
        {
            Messenger.Send(new OpenDetailViewEventArgs
            {
                Id = ((Measurement)Measurements.CurrentItem).Id,
                ViewModelName = nameof(MeasurementDetailViewModel)
            });
        }

        private void OnRemoveMeasurement()
        {
            if (Measurements.CurrentItem != null)
                _measurements.Remove((Measurement)Measurements.CurrentItem);
        }

        public override async Task LoadAsync(int measurementId, object argsData)
        {
            var testSubject = measurementId > 0
              ? await _testSubjectRepository.GetByIdAsync(measurementId)
              : CreateNewTestSubject();

            Id = measurementId;

            InitializeTestSubject(testSubject);

            _measurements = new ObservableCollection<Measurement>(TestSubject.Measurements);
            _measurements.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Measurements)); // TODO solve in different way
            Measurements = GetNewCollectionViewInstance(_measurements);
            Measurements.SortDescriptions.Add(new SortDescription(nameof(Measurement.Date), ListSortDirection.Ascending));
            Measurements.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Measurement.Type)));
        }

        private void InitializeTestSubject(TestSubject testSubject)
        {
            TestSubject = new TestSubjectWrapper(testSubject);
            TestSubject.PropertyChanged += (_, e) =>
          {
              // TODO REFACTORING !!!
              if (!HasChanges)
              {
                  HasChanges = _testSubjectRepository.HasChanges();
              }

              if (e.PropertyName == nameof(testSubject.FirstName) || e.PropertyName == nameof(testSubject.LastName))
              {
                  SetTitle();
              }
          };
            if (TestSubject.Id == 0)
            {
                // trigger the validation
                TestSubject.FirstName = "";
            }
            SetTitle();
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
