using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac.Features.Indexed;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private int nextNewItemId = 0;
        private readonly IMessenger _messenger;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IIndex<string, IDetailViewModel> _detailViewModelCreator; // TODO change


        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        public ICommand CreateNewDetailCommand { get; }

        public ICommand OpenSingleDetailViewCommand { get; }

        public INavigationViewModel NavigationViewModel { get; }

        public ObservableCollection<IDetailViewModel> DetailViewModels { get; }


        public IDetailViewModel SelectedDetailViewModel { get; set; }

        /// <summary>
        /// Design-time ctor
        /// </summary>
        public MainViewModel()
        {
        }


        public MainViewModel(INavigationViewModel navigationViewModel,
            IIndex<string, IDetailViewModel> detailViewModelCreator,
            IMessenger messenger,
            IMessageDialogService messageDialogService)
        {
            _messenger = messenger;
            _detailViewModelCreator = detailViewModelCreator;
            _messageDialogService = messageDialogService;

            DetailViewModels = new ObservableCollection<IDetailViewModel>();

            _messenger.Register<OpenDetailViewEventArgs>(this, (s, e) => OnOpenDetailView(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
            _messenger.Register<AfterDetailClosedEventArgs>(this, (s, e) => AfterDetailClosed(e));

            CreateNewDetailCommand = new RelayCommand<Type>(OnCreateNewDetailExecute);
            OpenSingleDetailViewCommand = new RelayCommand<Type>(OnOpenSingleDetailViewExecute);

            NavigationViewModel = navigationViewModel;
        }

        private async void OnOpenDetailView(OpenDetailViewEventArgs args)
        {
            var detailViewModel = DetailViewModels
              .SingleOrDefault(vm => vm.Id == args.Id
              && vm.GetType().Name == args.ViewModelName);

            if (detailViewModel == null)
            {
                detailViewModel = _detailViewModelCreator[args.ViewModelName];
                await detailViewModel.LoadAsync(args.Id);
                DetailViewModels.Add(detailViewModel);
            }

            SelectedDetailViewModel = detailViewModel;
        }

        private void OnCreateNewDetailExecute(Type viewModelType)
        {
            OnOpenDetailView(new OpenDetailViewEventArgs
            {
                Id = nextNewItemId--,
                ViewModelName = viewModelType.Name
            });
        }

        private void OnOpenSingleDetailViewExecute(Type viewModelType)
        {
            OnOpenDetailView(
           new OpenDetailViewEventArgs
           {
               Id = -1,
               ViewModelName = viewModelType.Name // TODO change
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
            var detailViewModel = DetailViewModels
                   .SingleOrDefault(vm => vm.Id == id
                   && vm.GetType().Name == viewModelName);
            if (detailViewModel != null)
            {
                DetailViewModels.Remove(detailViewModel);
            }
        }
    }
}
