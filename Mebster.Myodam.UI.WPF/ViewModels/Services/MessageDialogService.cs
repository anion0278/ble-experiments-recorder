using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services
{
    public enum MessageDialogResult
    {
        OK,
        Cancel,
        Yes,
        No
    }

    public class MessageDialogService : IMessageDialogService
    {
        private MetroWindow MetroWindow => (MetroWindow)App.Current.MainWindow;

        public async Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title)
        {
            var result = await MetroWindow.ShowMessageAsync(title, text, MessageDialogStyle.AffirmativeAndNegative);

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
              ? MessageDialogResult.OK
              : MessageDialogResult.Cancel;
        }

        public MessageDialogResult ShowOkCancelDialog(string text, string title)
        {
            var result = MetroWindow.ShowModalMessageExternal(title, text, MessageDialogStyle.AffirmativeAndNegative);

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
                ? MessageDialogResult.OK
                : MessageDialogResult.Cancel;
        }

        public MessageDialogResult ShowYesNoDialog(string text, string title)
        {
            var dialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                AnimateShow = true,
                AnimateHide = false,
                DialogMessageFontSize = 12,
            };

            var result = MetroWindow.ShowModalMessageExternal(title, text, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
                ? MessageDialogResult.Yes
                : MessageDialogResult.No;
        }

        public async Task ShowInfoDialogAsync(string text)
        {
            await MetroWindow.ShowMessageAsync("Info", text);
        }

        public void ShowInfoDialog(string text)
        {
            MetroWindow.ShowModalMessageExternal("Info", text);
        }

    }
}
