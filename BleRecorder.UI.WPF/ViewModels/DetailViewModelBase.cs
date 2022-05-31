﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.View.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.ViewModels
{
    public abstract class DetailViewModelBase : ViewModelBase, IDetailViewModel
    {
        private bool _hasChanges;
        protected readonly IMessenger EventAggregator;
        protected readonly IMessageDialogService MessageDialogService;

        public abstract Task LoadAsync(int measurementId);

        public ICommand SaveCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand CloseDetailViewCommand { get; }

        public int Id { get; protected set; }

        public virtual string Title { get; protected set; }

        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                if (_hasChanges != value)
                {
                    _hasChanges = value;
                    ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public DetailViewModelBase(IMessenger eventAggregator,
            IMessageDialogService messageDialogService)
        {
            EventAggregator = eventAggregator;
            MessageDialogService = messageDialogService;
            SaveCommand = new RelayCommand(OnSaveExecute, OnSaveCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteExecute);
            CloseDetailViewCommand = new RelayCommand(OnCloseDetailViewExecute);
        }

        protected abstract void OnDeleteExecute();

        protected abstract bool OnSaveCanExecute();

        protected abstract void OnSaveExecute();

        protected virtual void RaiseDetailDeletedEvent(int modelId)
        {
            EventAggregator.Send(new AfterDetailDeletedEventArgs
                {
                    Id = modelId,
                    ViewModelName = this.GetType().Name
                });
        }

        protected virtual void RaiseDetailSavedEvent(int modelId, string displayMember)
        {
            EventAggregator.Send(new AfterDetailSavedEventArgs
            {
                Id = modelId,
                DisplayMember = displayMember,
                ViewModelName = this.GetType().Name
            });
        }

        //protected virtual void RaiseCollectionSavedEvent()
        //{
        //    EventAggregator.GetEvent<AfterCollectionSavedEvent>()
        //      .Publish(new AfterCollectionSavedEventArgs
        //      {
        //          ViewModelName = this.GetType().Name
        //      });
        //}

        protected virtual async void OnCloseDetailViewExecute()
        {
            if (HasChanges)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync(
                  "You've made changes. Close this item?", "Question");
                if (result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }

            EventAggregator.Send(new AfterDetailClosedEventArgs
              {
                  Id = this.Id,
                  ViewModelName = this.GetType().Name
              });
        }

        protected async Task SaveAsync(Func<Task> saveFunc, Action afterSaveAction)
        {
            await saveFunc();
            afterSaveAction();
        }

    }
}
