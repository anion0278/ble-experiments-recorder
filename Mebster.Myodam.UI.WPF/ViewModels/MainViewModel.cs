using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autofac.Features.Indexed;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private int nextNewItemId = 0;
        private readonly IMessenger _messenger;
        private readonly IMyodamManager _myodamManager;
        private readonly IStimulationParametersRepository _stimulationParametersRepository;
        private readonly IMessageDialogService _dialogService;
        private readonly IIndex<string, IDetailViewModel> _detailViewModelCreator; // TODO change
        private IDetailViewModel _selectedDetailViewModel;
        private readonly ObservableCollection<IDetailViewModel> _detailViewModels = new();

        public StimulationParametersViewModel StimulationParametersViewModel { get; set; }

        public ICommand OpenSingleDetailViewCommand { get; }
        public RelayCommand<CancelEventArgs> MainViewClosingCommand { get; set; }

        public INavigationViewModel NavigationViewModel { get; }


        public ICollectionView DetailViewModels { get; }

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);


        public IDetailViewModel SelectedDetailViewModel
        {
            get => _selectedDetailViewModel;
            set
            {
                if (_selectedDetailViewModel == value
                    || (_myodamManager.IsCurrentlyMeasuring && !_myodamManager.MyodamDevice!.IsCalibrating))
                {
                    _dialogService.ShowInfoDialog("Please, finish measurement first.");
                    return;
                }
                _selectedDetailViewModel = value;
            }
        }


        /// <summary>
        /// Design-time ctor
        /// </summary>
        [Obsolete("Design-time only!")]
        public MainViewModel()
        {
            var ts = new TestSubject() { FirstName = "Subject", LastName = "Name", Notes = "Notes:\n   * Note1\n   * Note2" };
            _detailViewModels.Add(new TestSubjectDetailViewModel() { Model = ts });

            NavigationViewModel = new NavigationViewModel(ts);
        }

        public MainViewModel(INavigationViewModel navigationViewModel,
            IIndex<string, IDetailViewModel> detailViewModelCreator,
            IMessenger messenger,
            IMyodamManager myodamManager,
            IStimulationParametersRepository stimulationParametersRepository,
            IMessageDialogService dialogService)
        {
            _messenger = messenger;
            _myodamManager = myodamManager;
            _stimulationParametersRepository = stimulationParametersRepository;
            _detailViewModelCreator = detailViewModelCreator;
            _dialogService = dialogService;

            DetailViewModels = CollectionViewSource.GetDefaultView(_detailViewModels);

            _messenger.Register<OpenDetailViewEventArgs>(this, (s, e) => OnOpenDetailViewAsync(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
            _messenger.Register<AfterDetailClosedEventArgs>(this, (s, e) => AfterDetailClosed(e));

            OpenSingleDetailViewCommand = new RelayCommand(OpenSingleDetailViewExecute);
            MainViewClosingCommand = new RelayCommand<CancelEventArgs>(MainViewClosingExecute);

            NavigationViewModel = navigationViewModel;
        }

        private async void MainViewClosingExecute(CancelEventArgs? cancelEventArgs)
        {
            if (_detailViewModels.Any(vm => vm.HasChanges))
            {
                var result = _dialogService.ShowOkCancelDialog(
                    "There are unsaved changes. Do you really want to close the application?",
                    "Closing application");
                if (result == MessageDialogResult.Cancel)
                {
                    cancelEventArgs!.Cancel = true;
                    return;
                }
            }

            StimulationParametersViewModel.PropertyChanged -= StimulationParametersViewModel_PropertyChanged;

            if (_myodamManager.MyodamDevice is null) return;
            var result_fes = _dialogService.ShowYesNoDialog(
                "Do you want to disable FES module?",
                "Disabling FES module");

            if (_myodamManager.MyodamDevice is null) return;// status could have changed in meantime!

            if (result_fes == MessageDialogResult.Yes)
            {
                await _myodamManager.MyodamDevice.DisableFesAndDisconnectAsync();
                Debug.Print("Exit and disable FES");
                return;
            }
            await _myodamManager.MyodamDevice.DisconnectAsync();
            Debug.Print("Exit");
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
            StimulationParametersViewModel = new StimulationParametersViewModel(await _stimulationParametersRepository.GetByIdAsync(1));
            StimulationParametersViewModel.PropertyChanged += StimulationParametersViewModel_PropertyChanged;
        }

        private async void StimulationParametersViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            await _stimulationParametersRepository.SaveAsync();
        }

        private async void OnOpenDetailViewAsync(OpenDetailViewEventArgs args)
        {
            if (_myodamManager.IsCurrentlyMeasuring)
            {
                await _dialogService.ShowInfoDialogAsync("Please, finish measurement first.");
                return;
            }

            var detailViewModel = _detailViewModels
              .SingleOrDefault(vm => vm.Id == args.Id && vm.GetType().Name == args.ViewModelName);

            if (detailViewModel == null)
            {
                detailViewModel = _detailViewModelCreator[args.ViewModelName];
                await detailViewModel.LoadAsync(args.Id, args.Data);
                _detailViewModels.Add(detailViewModel);
            }

            DetailViewModels.MoveCurrentTo(detailViewModel);
        }

        private void OpenSingleDetailViewExecute()
        {
            OnOpenDetailViewAsync(new OpenDetailViewEventArgs
            {
                Id = -1,
                ViewModelName = nameof(TestSubjectDetailViewModel)
            });
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        private void AfterDetailClosed(AfterDetailClosedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        private void RemoveDetailViewModel(int id, string viewModelName)
        {
            // TODO forbid removing tab during measurement
            //if (_myodamManager.IsCurrentlyMeasuring)
            //{
            //    await _dialogService.ShowInfoDialogAsync("Please finish measurement first.");
            //    return;
            //}

            var detailViewModel = _detailViewModels
                   .Single(vm => vm.Id == id && vm.GetType().Name == viewModelName);

            _detailViewModels.Remove(detailViewModel);
        }
    }
}
