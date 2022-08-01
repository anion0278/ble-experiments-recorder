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
using AutoMapper;
using Castle.Components.DictionaryAdapter;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.Views.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class TestSubjectDetailViewModel : DetailViewModelBase, ITestSubjectDetailViewModel
    {
        private ITestSubjectRepository _testSubjectRepository;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IMapper _mapper;

        public ICommand RemoveMeasurementCommand { get; set; }

        public ICommand EditMeasurementCommand { get; set; }

        public ICommand AddMeasurementCommand { get; set; }

        private ObservableCollection<Measurement> _measurements;

        public ICollectionView Measurements { get; set; }

        public TestSubject Model { get; set; } // MUST BE PUBLIC PROP in order to make validation work on init

        public MechanismParametersViewModel MechanismParametersVm { get; private set; }
        public StimulationParametersViewModel StimulationParametersVm { get; private set; }

        public override string Title => $"{FirstName} {LastName}";

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string FirstName
        {
            get => Model.FirstName;
            set => Model.FirstName = value;
        }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string LastName
        {
            get => Model.LastName;
            set => Model.LastName = value;
        }

        public string Notes
        {
            get => Model.Notes;
            set => Model.Notes = value;
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public TestSubjectDetailViewModel() : base(null, null)
        {
            _measurements = new ObservableCollection<Measurement>() { new Measurement() {Title = "Measurement 1"}};
            Measurements = CollectionViewSource.GetDefaultView(_measurements);
        }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMeasurementRepository measurementRepository,
            IMessenger messenger,
            IMapper mapper,
            IMessageDialogService messageDialogService)
          : base(messenger, messageDialogService)
        {
            _testSubjectRepository = testSubjectRepository;
            _measurementRepository = measurementRepository;
            _mapper = mapper;

            AddMeasurementCommand = new RelayCommand(async () => await OnAddMeasurement());
            EditMeasurementCommand = new RelayCommand(OnEditMeasurement, () => Measurements!.CurrentItem != null);
            RemoveMeasurementCommand = new RelayCommand(OnRemoveMeasurement, () => Measurements!.CurrentItem != null);
            
            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailChanged(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailChanged(e));
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

            MechanismParametersVm = new MechanismParametersViewModel(new MechanismParameters(Model.CustomizedAdjustments));
            MechanismParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            StimulationParametersVm = new StimulationParametersViewModel(Model.CustomizedParameters);
            StimulationParametersVm.PropertyChanged += OnPropertyChangedEventHandler;

            PropertyChanged += OnPropertyChangedEventHandler;
        }

        protected override void UnsubscribeOnClosing()
        {
            MechanismParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            StimulationParametersVm.PropertyChanged -= OnPropertyChangedEventHandler;
            PropertyChanged -= OnPropertyChangedEventHandler;

            //_measurements.CollectionChanged -= (_, _) => OnPropertyChanged(nameof(Measurements)); // TODO
            Messenger.Unregister<AfterDetailSavedEventArgs>(this);
            Messenger.Unregister<AfterDetailDeletedEventArgs>(this);
        }

        private void OnPropertyChangedEventHandler(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // An alternative to mapping could have been a ParamValue type, which however has a disadvantage - it should be immutable VO, which makes it inappropriate
            _mapper.Map(MechanismParametersVm.Model.GetCurrentAdjustments(), Model.CustomizedAdjustments);

            HasChanges = _testSubjectRepository.HasChanges();
        }

        private async void AfterDetailChanged(IDetailViewEventArgs message)
        {
            if (message.ViewModelName != nameof(MeasurementDetailViewModel)) return;

            var relatedMeas = await _measurementRepository.GetByIdAsync(message.Id);
            if (relatedMeas.TestSubjectId != Id) return;

            Model = await _testSubjectRepository.ReloadAsync(Model);

            _measurements.Clear();
            foreach (var measurement in Model.Measurements) // TODO fix BUG !! except those which have been deleted!
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
            if (Measurements.CurrentItem == null) return;

            _testSubjectRepository.RemoveMeasurement((Measurement)Measurements.CurrentItem);
            _measurements.Remove((Measurement)Measurements.CurrentItem);
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
            if (Id < 0)
            {
                OnCloseDetailViewExecute();
                return;
            }

            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the test subject {Title} and all related measurements?", "Confirmation is required");

            if (result != MessageDialogResult.OK) return;

            _testSubjectRepository.Remove(Model);
            await _testSubjectRepository.SaveAsync();
            RaiseDetailDeletedEvent(Model.Id);
        }

        private TestSubject CreateNewTestSubject()
        {
            var testSubject = new TestSubject
            {
                CustomizedAdjustments = new DeviceMechanicalAdjustments(),
                CustomizedParameters = StimulationParameters.GetDefaultValues()
            };

            _testSubjectRepository.Add(testSubject);
            return testSubject;
        }
    }
}
