using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Business.Device;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.UI.WPF.ViewModels
{
    public abstract class DetailViewModelBase : ViewModelBase, IDetailViewModel
    {
        private bool _hasChanges;
        protected readonly IMessenger Messenger;
        protected readonly IMessageDialogService DialogService;
        private readonly IBleRecorderManager _bleRecorderManager;

        public abstract Task LoadAsync(int measurementId, object argsData);

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand DeleteCommand { get; }

        public ICommand CloseDetailViewCommand { get; }

        public int Id { get; protected set; }

        public bool IsActive { get; set; }

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

        protected DetailViewModelBase(
            IMessenger messenger, 
            IMessageDialogService dialogService,
            IBleRecorderManager bleRecorderManager)
        {
            Messenger = messenger;
            DialogService = dialogService;
            _bleRecorderManager = bleRecorderManager;
            SaveCommand = new RelayCommand(OnSaveExecuteAsync, OnSaveCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteExecuteAsync, OnDeleteCanExecute);
            CloseDetailViewCommand = new RelayCommand(OnCloseDetailViewExecuteAsync);
        }

        protected virtual bool OnDeleteCanExecute()
        {
            return true;
        }

        protected abstract void OnDeleteExecuteAsync();

        protected abstract void OnSaveExecuteAsync();

        protected virtual bool OnSaveCanExecute()
        {
            return !HasErrors && HasChanges;
        }

        protected virtual void RaiseDetailDeletedEvent(int modelId)
        {
            CleanupOnClosing();
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

        protected abstract void CleanupOnClosing();

        protected async void OnCloseDetailViewExecuteAsync()
        {
            if (!await UserAcknowledgedClosingAsync()) return;

            RaiseDetailClosedEvent();
        }

        protected void RaiseDetailClosedEvent()
        {
            CleanupOnClosing();
            Messenger.Send(new AfterDetailClosedEventArgs 
            {
                Id = this.Id,
                ViewModelName = GetType().Name
            });
        }

        protected virtual async Task<bool> UserAcknowledgedClosingAsync()
        {
            if (!HasChanges) return true;

            var result = await DialogService.ShowOkCancelDialogAsync(
                "Close this item? Changes will be lost.", "Question");
            return result == MessageDialogResult.OK;
        }
    }
}
