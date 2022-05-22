using System.Threading.Tasks;

namespace Mebster.Myodam.UI.WPF.View.Services
{
  public interface IMessageDialogService
  {
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    Task ShowInfoDialogAsync(string text);
  }
}