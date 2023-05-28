using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Measurements;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;
using PropertyChanged;

namespace BleRecorder.UI.WPF.TestSubjects
{
    public class TestSubjectDetailViewModel : DetailViewModelBase, ITestSubjectDetailViewModel
    {
        private readonly IList<Measurement> _removedMeasurements = new List<Measurement>();
        private readonly ObservableCollection<Measurement> _measurements;
        private readonly ITestSubjectRepository _testSubjectRepository;
        private readonly IMapper _mapper;
        private readonly IStimulationParametersRepository _stimulationParametersRepository;

        public ICommand RemoveMeasurementCommand { get; set; }

        public ICommand EditMeasurementCommand { get; set; }

        public ICommand AddMeasurementCommand { get; set; }


        public ChartValues<StatisticsValue> MaxContractionStatisticValues { get; set; } = new();
        public ChartValues<StatisticsValue> IntermittentStatisticValues { get; set; } = new();

        public ICollectionView Measurements { get; set; }

        public Models.TestSubject.TestSubject Model { get; set; } // MUST BE PUBLIC PROP in order to make validation work on init

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
            _measurements = new ObservableCollection<Measurement>() { new Measurement() { Title = "Measurement 1" } };
            Measurements = CollectionViewSource.GetDefaultView(_measurements);
        }

        public TestSubjectDetailViewModel(
            ITestSubjectRepository testSubjectRepository,
            IMessenger messenger,
            IMapper mapper,
            IBleRecorderManager bleRecorderManager,
            IStimulationParametersRepository stimulationParametersRepository,
            IMessageDialogService dialogService)
          : base(messenger, dialogService, bleRecorderManager)
        {
            _testSubjectRepository = testSubjectRepository; // TODO possible refactoring - this is main repo for VM, abstract it along with related methods
            _mapper = mapper;
            _stimulationParametersRepository = stimulationParametersRepository;

            AddMeasurementCommand = new RelayCommand(async () => await OnAddMeasurementAsync());
            EditMeasurementCommand = new RelayCommand(OnEditMeasurement, () => Measurements!.CurrentItem != null);
            RemoveMeasurementCommand = new RelayCommand(OnRemoveMeasurement, () => Measurements!.CurrentItem != null);

            Messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailChanged(e));
            Messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailChanged(e));

            _measurements = new ObservableCollection<Measurement>();
            _measurements.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Measurements)); // TODO why is it required?
            Measurements = CollectionViewSource.GetDefaultView(_measurements);
            Measurements.SortDescriptions.Add(new SortDescription(nameof(Measurement.Date), ListSortDirection.Ascending));
            Measurements.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Measurement.Type)));
            Measurements.MoveCurrentTo(null);
        }


        public override async Task LoadAsync(int id, object argsData)
        {
            Model = id > 0
                ? await _testSubjectRepository.GetByIdAsync(id)
                : await CreateNewTestSubject();

            Id = id;
            if (id > 0) await RefreshMeasurements(); // TODO refactor

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
            // TODO reload only if changed VM is related to this TS
            if (message.ViewModelName != nameof(MeasurementDetailViewModel)) return;

            await RefreshMeasurements();
        }

        private async Task RefreshMeasurements()
        {
            Model = await _testSubjectRepository.ReloadAsync(Model);

            _measurements.Clear();
            foreach (var measurement in Model.Measurements.Except(_removedMeasurements)) 
            {
                _measurements.Add(measurement);
            }

            RefreshStatistics();
        }

        private void RefreshStatistics()
        {
            MaxContractionStatisticValues.Clear();
            MaxContractionStatisticValues.AddRange(GetStatisticsValues(
                MeasurementType.MaximumContraction,
                m => new StatisticsValue(m.ContractionLoadData.Max(v => v.ContractionValue), m.Date!.Value)));
            OnPropertyChanged(nameof(MaxContractionStatisticValues)); // only one update is enough, since MultiBinding will be triggered for both statements

            IntermittentStatisticValues.Clear();
            IntermittentStatisticValues.AddRange(GetStatisticsValues(
                MeasurementType.Intermittent,
                m => new StatisticsValue(m.Intermittent.Value, m.Date!.Value)));
            OnPropertyChanged(nameof(IntermittentStatisticValues));
        }

        private IEnumerable<StatisticsValue> GetStatisticsValues(MeasurementType measurementType, Func<Measurement, StatisticsValue> selector) // TODO into statistics Service
        {
            var statisticDataGroupedByDateOnly = _measurements
                .Where(m => m.Type == measurementType && m.ContractionLoadData.Any() && m.Date.HasValue)
                .Select(selector)
                .GroupBy(d => d.MeasurementDate.Date);

            return statisticDataGroupedByDateOnly
                .Select(g => g.MaxBy(stat => stat.Value))
                .OrderBy(sv => sv!.MeasurementDate)!;
        }

        private async Task OnAddMeasurementAsync()
        {
            if (await _testSubjectRepository.GetByIdAsync(Id) == null || HasChanges)
            {
                await DialogService.ShowInfoDialogAsync(
                    "Changes are not saved. Please, save changes before adding measurements.");
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

            var measToRemove = (Measurement)Measurements.CurrentItem;

            _testSubjectRepository.RemoveMeasurement(measToRemove);
            _measurements.Remove(measToRemove);
            _removedMeasurements.Add(measToRemove);

            RefreshStatistics();
        }

        protected override async void OnSaveExecuteAsync()
        {
            if ((await _testSubjectRepository.GetAllAsync()) // TODO replace getAll with customized query
                .Any<Models.TestSubject.TestSubject>(ts => ts.FullName == Model.FullName && ts.Id != Model.Id))
            {
                await DialogService.ShowInfoDialogAsync($"A test subject with name '{Model.FullName}' already exists. Please change the name.");
                return;
            }

            await _testSubjectRepository.SaveAsync();
            _removedMeasurements.Clear();
            Id = Model.Id;
            HasChanges = false;
            RaiseDetailSavedEvent(Model.Id, $"{Model.FirstName} {Model.LastName}");
        }

        protected override async void OnDeleteExecuteAsync()
        {
            if (Id < 0)
            {
                OnCloseDetailViewExecuteAsync();
                return;
            }

            var result = await DialogService.ShowOkCancelDialogAsync(
                $"Do you really want to delete the test subject {Title} and all related measurements?", "Confirmation is required");
            if (result != MessageDialogResult.OK) return;

            _testSubjectRepository.Remove(Model);
            await _testSubjectRepository.SaveAsync();
            RaiseDetailDeletedEvent(Model.Id);
        }

        private async Task<Models.TestSubject.TestSubject> CreateNewTestSubject()
        {
            var testSubject = new Models.TestSubject.TestSubject
            {
                CustomizedAdjustments = new DeviceMechanicalAdjustments(),
                CustomizedParameters = (StimulationParameters)(await _stimulationParametersRepository.GetByIdAsync(1))!.Clone()
            };

            _testSubjectRepository.Add(testSubject);
            return testSubject;
        }
    }
}
