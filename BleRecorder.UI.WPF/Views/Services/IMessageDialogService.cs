using System.Threading.Tasks;

namespace BleRecorder.UI.WPF.Views.Services
{
  public interface IMessageDialogService
  {
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    Task ShowInfoDialogAsync(string text);
  }
}