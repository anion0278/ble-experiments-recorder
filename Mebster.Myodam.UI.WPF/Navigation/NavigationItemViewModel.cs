using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Navigation
{
    public interface INavigationItemViewModel : INotifyPropertyChanged // INPC is required to make BindingList propagate the changes
    {
        TestSubject Model { get; }
        bool IsSelected { get; set; }
        string DisplayMember { get; set; }
        bool HasErrors { get; }
        int Id { get; }
        ICommand OpenDetailViewCommand { get; }
    }

    // TODO remove, since no longer needed
    public class NavigationItemViewModel : NavigationAddTestSubjectItemViewModel, INavigationItemViewModel
    {
        public TestSubject Model { get; }

        public virtual bool IsSelected { get; set; }

        public string DisplayMember { get; set; }

        public NavigationItemViewModel(TestSubject testSubject, IMessenger messenger): base(messenger)
        {
            Id = testSubject.Id;
            DisplayMember = testSubject.FullName;
            Model = testSubject;
        }
    }

    // TODO remove, since no longer used
    public class NavigationAddTestSubjectItemViewModel :  ViewModelBase
    {
        private readonly IMessenger _messenger;

        public int Id { get; protected set; }

        public ICommand OpenDetailViewCommand { get; }

        public NavigationAddTestSubjectItemViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            Id = -999;
            OpenDetailViewCommand = new RelayCommand(OnOpenDetailViewExecute);
        }

        private void OnOpenDetailViewExecute()
        {
            _messenger.Send(new OpenDetailViewEventArgs
            {
                Id = Id,
                ViewModelName = nameof(TestSubjectDetailViewModel)
            });
        }
    }
}
