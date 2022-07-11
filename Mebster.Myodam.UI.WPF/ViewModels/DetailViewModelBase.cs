using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public abstract class DetailViewModelBase : ViewModelBase, IDetailViewModel
    {
        private bool _hasChanges;
        protected readonly IMessenger Messenger;
        protected readonly IMessageDialogService MessageDialogService;

        public abstract Task LoadAsync(int measurementId, object argsData);

        public IRelayCommand SaveCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand CloseDetailViewCommand { get; }

        public int Id { get; protected set; }

        public virtual string Title { get; set; }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (_hasChanges == value) return;
                _hasChanges = value;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        protected DetailViewModelBase(IMessenger messenger, IMessageDialogService messageDialogService)
        {
            Messenger = messenger;
            MessageDialogService = messageDialogService;
            SaveCommand = new RelayCommand(OnSaveExecute, OnSaveCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteExecute);
            CloseDetailViewCommand = new RelayCommand(OnCloseDetailViewExecute);
        }

        protected abstract void OnDeleteExecute();

        protected abstract void OnSaveExecute();

        protected virtual bool OnSaveCanExecute()
        {
            return !HasErrors && HasChanges;
        }

        protected virtual void RaiseDetailDeletedEvent(int modelId)
        {
            UnsubscribeOnClosing();
            Messenger.Send(new AfterDetailDeletedEventArgs
            {
                Id = modelId,
                ViewModelName = GetType().Name
            });
        }

        protected virtual void RaiseDetailSavedEvent(int modelId, string displayMember)// TODO check if these are used correctly everywhere
        {
            Messenger.Send(new AfterDetailSavedEventArgs
            {
                Id = modelId,
                DisplayMember = displayMember,
                ViewModelName = GetType().Name
            });
        }

        protected abstract void UnsubscribeOnClosing();

        protected virtual async void OnCloseDetailViewExecute()
        {
            if (!await UserAcknowledgedClosing()) return;

            RaiseDetailClosedEvent();
        }

        protected void RaiseDetailClosedEvent()
        {
            UnsubscribeOnClosing();
            Messenger.Send(new AfterDetailClosedEventArgs 
            {
                Id = this.Id,
                ViewModelName = GetType().Name
            });
        }

        protected virtual async Task<bool> UserAcknowledgedClosing()
        {
            if (!HasChanges) return true;

            var result = await MessageDialogService.ShowOkCancelDialogAsync(
                "Close this item? Changes will be lost.", "Question");
            return result == MessageDialogResult.OK;
        }
    }
}
