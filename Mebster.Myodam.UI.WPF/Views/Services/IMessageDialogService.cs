using System.Threading.Tasks;

namespace Mebster.Myodam.UI.WPF.Views.Services
{
  public interface IMessageDialogService
  {
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    Task ShowInfoDialogAsync(string text);
  }
}