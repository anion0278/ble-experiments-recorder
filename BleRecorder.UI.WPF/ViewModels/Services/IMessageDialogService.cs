using System.Threading.Tasks;

namespace BleRecorder.UI.WPF.ViewModels.Services
{
  public interface IMessageDialogService
  {
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    MessageDialogResult ShowOkCancelDialog(string text, string title);
    MessageDialogResult ShowYesNoDialog(string text, string title);
    Task ShowInfoDialogAsync(string text);
    void ShowInfoDialog(string text);
  }
}