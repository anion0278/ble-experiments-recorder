﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Models.TestSubjects;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.TestSubjects;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Navigation
{
    public class NavigationItemViewModel :  ViewModelBase, INavigationItemViewModel
    {
        private readonly IMessenger _messenger;

        public TestSubject Model { get; }

        public virtual bool IsSelectedForExport { get; set; }

        public string ItemName { get; protected set; }

        public int Id => Model.Id;

        public ICommand OpenDetailViewCommand { get; }

        public NavigationItemViewModel(TestSubject testSubject, IMessenger messenger)
        {
            _messenger = messenger;
            Model = testSubject;
            ItemName = testSubject.FullName;
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            OpenDetailViewCommand = new RelayCommand(OnOpenDetailViewExecute);
        }

        private void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel) && args.Id == Model.Id)
            {
                ItemName = args.DisplayMember;
            }
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
