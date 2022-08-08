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
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;

namespace BleRecorder.UI.WPF.ViewModels
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

        public override string Title => string.IsNullOrWhiteSpace(Model.FullName) ? "(New subject)" : Model.FullName;

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [AlsoNotifyFor(nameof(Title))]
        public string FirstName
        {
            get => Model.FirstName;
            set => Model.FirstName = value;
        }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [AlsoNotifyFor(nameof(Title))]
        public string LastName
        {
            get => Model.LastName;
            set => Model.LastName = value;
        }

        public string? Notes
        {
            get => Model.Notes;
            set => Model.Notes = value;
        }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public TestSubjectDetailViewModel() : base(null!, null!, null!)
        {
            _measurements = new ObservableCollection<Measurement>() { new Measurement() {Title = "Measurement 1"}};
            Measurements = CollectionViewSource.GetDefaultView(_measurements);
        }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMeasurementRepository measurementRepository,
            IMessenger messenger,
            IMapper mapper,
            IBleRecorderManager bleRecorderManager,
            IMessageDialogService dialogService)
          : base(messenger, dialogService, bleRecorderManager)
        {
            _testSubjectRepository = testSubjectRepository; // TODO possible refactoring - this is main repo for VM, abstract it along with related methods
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

            MechanismParametersVm = new MechanismParametersViewModel(new MechanismParameters(Model.CustomizedAdjustments), _mapper);
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
            MechanismParametersVm.CopyAdjustmentValuesTo(Model.CustomizedAdjustments);
            HasChanges = _testSubjectRepository.HasChanges();
        }

        private async void AfterDetailChanged(IDetailViewEventArgs message)
        {
            if (message.ViewModelName != nameof(MeasurementDetailViewModel)) return;

            Model = await _testSubjectRepository.ReloadAsync(Model);

            _measurements.Clear();
            foreach (var measurement in Model.Measurements) // TODO fix BUG !! except those which have been deleted!
            {
                _measurements.Add(measurement);
            }
        }

        private async Task OnAddMeasurement()
        {
            if (await _testSubjectRepository.GetByIdAsync(Id) == null || HasChanges)
            {
                await DialogService.ShowInfoDialogAsync(
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
                ViewModelName = nameof(MeasurementDetailViewModel),
                Data = Model
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
            if ((await _testSubjectRepository.GetAllAsync()) // TODO replace getAll with customized query
                .Any(ts => ts.FullName == Model.FullName && ts.Id != Model.Id))
            {
                await DialogService.ShowInfoDialogAsync($"A test subject with name '{Model.FullName}' already exists. Please change the name.");
                return;
            }

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

            var result = await DialogService.ShowOkCancelDialogAsync(
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
