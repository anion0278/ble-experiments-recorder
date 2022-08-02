using System.Windows.Input;
using Mebster.Myodam.UI.WPF.Event;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationItemViewModel : ViewModelBase
    {
        private IMessenger _messenger;
        private string _detailViewModelName;

        public int Id { get; }

        public string DisplayMember { get; set; }

        public ICommand OpenDetailViewCommand { get; }

        public NavigationItemViewModel(int id, string displayMember, string detailViewModelName, IMessenger messenger)
        {
            _messenger = messenger;
            Id = id;
            DisplayMember = displayMember;
            _detailViewModelName = detailViewModelName;
            OpenDetailViewCommand = new RelayCommand(OnOpenDetailViewExecute);
        }

        private void OnOpenDetailViewExecute()
        {
            _messenger.Send(new OpenDetailViewEventArgs
              {
                  Id = Id,
                  ViewModelName = _detailViewModelName
              });
        }
    }
}
