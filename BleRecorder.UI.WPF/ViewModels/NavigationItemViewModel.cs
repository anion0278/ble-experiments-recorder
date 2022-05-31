using System.Windows.Input;
using BleRecorder.UI.WPF.Event;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.ViewModels
{
    public class NavigationItemViewModel : ViewModelBase
    {
        private IMessenger _eventAggregator;
        private string _detailViewModelName;

        public int Id { get; }

        public string DisplayMember { get; set; }

        public ICommand OpenDetailViewCommand { get; }

        public NavigationItemViewModel(int id, string displayMember,
            string detailViewModelName,
            IMessenger eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Id = id;
            DisplayMember = displayMember;
            _detailViewModelName = detailViewModelName;
            OpenDetailViewCommand = new RelayCommand(OnOpenDetailViewExecute);
        }

        private void OnOpenDetailViewExecute()
        {
            _eventAggregator.Send(
              new OpenDetailViewEventArgs
              {
                  Id = Id,
                  ViewModelName = _detailViewModelName
              });
        }
    }
}
