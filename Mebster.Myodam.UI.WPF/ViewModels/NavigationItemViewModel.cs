using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Mebster.Myodam.UI.WPF.Event;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationItemViewModel : ViewModelBase
    {
        private IMessenger _messenger;
        private readonly string _detailViewModelName = nameof(TestSubjectDetailViewModel);

        public int Id { get; }

        public string DisplayMember { get; set; }

        public ICommand OpenDetailViewCommand { get; }

        public NavigationItemViewModel(int id, string displayMember, IMessenger messenger)
        {
            _messenger = messenger;
            Id = id;
            DisplayMember = displayMember;
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

    public class NavigationAddItemViewModelRelationalComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xType = x.GetType();
            var yType = y.GetType();
            if (xType == typeof(NavigationItemViewModel) && yType == typeof(NavigationAddItemViewModel)) return -1;
            if (xType == typeof(NavigationAddItemViewModel) && yType == typeof(NavigationItemViewModel)) return 1;
            return 0;
        }
    }

    public class NavigationAddItemViewModel : NavigationItemViewModel
    {
        
        public NavigationAddItemViewModel(IMessenger messenger)
            : base(-999, "+ Add new test subject", messenger)
        {
        }
    }
}
