﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Event;

namespace BleRecorder.UI.WPF.ViewModels
{
    // TODO remove, since no longer needed
    public class NavigationTestSubjectItemViewModel : NavigationAddTestSubjectItemViewModel
    {
        public TestSubject Model { get; }

        public virtual bool IsSelected { get; set; }

        public string DisplayMember { get; set; }

        public NavigationTestSubjectItemViewModel(TestSubject testSubject, IMessenger messenger): base(messenger)
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

    public class NavigationAddItemViewModelRelationalComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xType = x.GetType();
            var yType = y.GetType();
            if (xType == typeof(NavigationTestSubjectItemViewModel) && yType == typeof(NavigationAddTestSubjectItemViewModel)) return -1;
            if (xType == typeof(NavigationAddTestSubjectItemViewModel) && yType == typeof(NavigationTestSubjectItemViewModel)) return 1;
            if (x is NavigationTestSubjectItemViewModel xVm && y is NavigationTestSubjectItemViewModel yVm)
            {
                return string.Compare(GetNameParameter(xVm), GetNameParameter(yVm), StringComparison.OrdinalIgnoreCase);
            }
            return 0;
        }

        // comparing by LastName and FirstName instead of FullName because it is more likely to be comfortable
        private static string GetNameParameter(NavigationTestSubjectItemViewModel vm)
        {
            return vm.Model.LastName + " " + vm.Model.FirstName;
        }
    }
}
