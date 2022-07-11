using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Castle.Components.DictionaryAdapter;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.View.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class TestSubjectDetailViewModel : DetailViewModelBase, ITestSubjectDetailViewModel
    {
        private ITestSubjectRepository _testSubjectRepository;
        private readonly IMeasurementRepository _measurementRepository;


        public ICommand RemoveMeasurementCommand { get; set; }

        public ICommand EditMeasurementCommand { get; set; }

        public ICommand AddMeasurementCommand { get; set; }

        private ObservableCollection<Measurement> _measurements;

        public ICollectionView Measurements { get; set; }

        public TestSubject Model { get; set; } // MUST BE PUBLIC PROP in order to make validation work on init


        [Required]
        [MinLength(2, ErrorMessage = "{0} should contain at least {1} characters.")]
        [MaxLength(20, ErrorMessage = "{0} should contain maximum of {1} characters.")]
        [Display(Name = "First name")]
        public string FirstName
        {
            get => Model.FirstName;
            set => Model.FirstName = value;
        }

        [Required]
        [MinLength(2, ErrorMessage = "{0} should contain at least {1} characters.")]
        [MaxLength(20, ErrorMessage = "{0} should contain maximum of {1} characters.")]
        [Display(Name = "Last name")]
        public string LastName
        {
            get => Model.LastName;
            set => Model.LastName = value;
        }

        public override string Title => $"{FirstName} {LastName}";

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public TestSubjectDetailViewModel() : base(null, null)
        { }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMeasurementRepository measurementRepository,
            IMessenger messenger,
            IMessageDialogService messageDialogService)
          : base(messenger, messageDialogService)
        {
            _testSubjectRepository = testSubjectRepository;
            _measurementRepository = measurementRepository;

            AddMeasurementCommand = new RelayCommand(async () => await OnAddMeasurement());
            EditMeasurementCommand = new RelayCommand(OnEditMeasurement, () => Measurements!.CurrentItem != null);
            RemoveMeasurementCommand = new RelayCommand(OnRemoveMeasurement, () => Measurements!.CurrentItem != null);

            PropertyChanged += OnPropertyChangedEventHandler;
            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailChanged(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailChanged(e));
        }


        protected override void UnsubscribeOnClosing()
        {
            PropertyChanged -= OnPropertyChangedEventHandler;
            //_measurements.CollectionChanged -= (_, _) => OnPropertyChanged(nameof(Measurements));
            Messenger.Unregister<AfterDetailSavedEventArgs>(this);
            Messenger.Unregister<AfterDetailDeletedEventArgs>(this);
        }

        private void OnPropertyChangedEventHandler(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            HasChanges = _testSubjectRepository.HasChanges();
            SaveCommand.NotifyCanExecuteChanged(); // TODO investigate why this is required when more than 3 tabs are open see #7
        }

        private async void AfterDetailChanged(IDetailViewEventArgs message)
        {
            if (message.ViewModelName != nameof(MeasurementDetailViewModel)) return;

            var relatedMeas = await _measurementRepository.GetByIdAsync(message.Id);
            if (relatedMeas.TestSubjectId != Id) return;

            Model = await _testSubjectRepository.ReloadAsync(Model);

            _measurements.Clear();
            foreach (var measurement in Model.Measurements) // TODO !! except those which have been deleted!
            {
                _measurements.Add(measurement);
            }
        }

        private async Task OnAddMeasurement()
        {
            if (await _testSubjectRepository.GetByIdAsync(Id) == null)
            {
                await MessageDialogService.ShowInfoDialogAsync(
                    "Test subject is not saved. Save changes before adding measurements.");
                return;
            }

            Messenger.Send(new OpenDetailViewEventArgs()
            {
                Id = -Id,
                ViewModelName = nameof(MeasurementDetailViewModel),
                Data = Model
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
            {
                _testSubjectRepository.RemoveMeasurement((Measurement)Measurements.CurrentItem);
                _measurements.Remove((Measurement)Measurements.CurrentItem);
            }
        }

        public override async Task LoadAsync(int id, object argsData)
        {
            Model = id > 0
              ? await _testSubjectRepository.GetByIdAsync(id)
              : CreateNewTestSubject();

            Id = id;

            _measurements = new ObservableCollection<Measurement>(Model.Measurements);
            _measurements.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Measurements)); // TODO why is it required?
            Measurements = CollectionViewSource.GetDefaultView(_measurements);
            Measurements.SortDescriptions.Add(new SortDescription(nameof(Measurement.Date), ListSortDirection.Ascending));
            Measurements.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Measurement.Type)));
            Measurements.MoveCurrentTo(null);
        }

        protected override async void OnSaveExecute()
        {
            await _testSubjectRepository.SaveAsync();
            Id = Model.Id;
            HasChanges = false;
            RaiseDetailSavedEvent(Model.Id, $"{Model.FirstName} {Model.LastName}");
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the test subject {Title} and all related measurements?", "Confirmation is required");

            if (result == MessageDialogResult.OK)
            {
                _testSubjectRepository.Remove(Model);
                await _testSubjectRepository.SaveAsync();
                RaiseDetailDeletedEvent(Model.Id);
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
