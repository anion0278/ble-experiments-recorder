using System.Diagnostics;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace BleRecorder.UI.WPF.ViewModels.Services
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

        private static MetroDialogSettings GetDialogSettings()
        {
            return new MetroDialogSettings()
            {
                AnimateShow = false,
                AnimateHide = false,
                DialogMessageFontSize = 16
            };
        }

        public async Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title)
        {
            var result = await MetroWindow.ShowMessageAsync(title, text, MessageDialogStyle.AffirmativeAndNegative, GetDialogSettings());

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
              ? MessageDialogResult.OK
              : MessageDialogResult.Cancel;
        }

        public MessageDialogResult ShowOkCancelDialog(string text, string title)
        {
            var result = MetroWindow.ShowModalMessageExternal(title, text, MessageDialogStyle.AffirmativeAndNegative, GetDialogSettings());

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
                ? MessageDialogResult.OK
                : MessageDialogResult.Cancel;
        }

        public MessageDialogResult ShowYesNoDialog(string text, string title)
        {
            var yesNoDialogSettings = GetDialogSettings();
            yesNoDialogSettings.AffirmativeButtonText = "Yes";
            yesNoDialogSettings.NegativeButtonText = "No";

            var result = MetroWindow.ShowModalMessageExternal(title, text, MessageDialogStyle.AffirmativeAndNegative, yesNoDialogSettings);

            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative
                ? MessageDialogResult.Yes
                : MessageDialogResult.No;
        }

        public async Task ShowInfoDialogAsync(string text)
        {
            await MetroWindow.ShowMessageAsync("Info", text, settings: GetDialogSettings());
        }

        public void ShowInfoDialog(string text)
        {
            MetroWindow.ShowModalMessageExternal("Info", text, settings: GetDialogSettings());
        }

        public bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName)
        {
            selectedFileName = null;
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export data",
                FileName = predefinedName,
                Filter = "Excel Document|*.xlsx", // TODO Builder/Factory
            };
            bool? dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != true) return false;

            selectedFileName = saveFileDialog.FileName;
            return true;
        }

        public void OpenOrShowDir(string dirName)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = dirName,
                Verb = "open", // utilizes opened folder if available
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
