using System.Threading.Tasks;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services
{
  public interface IMessageDialogService
  {
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    Task ShowInfoDialogAsync(string text);

    void ShowInfoDialog(string text);
  }
}