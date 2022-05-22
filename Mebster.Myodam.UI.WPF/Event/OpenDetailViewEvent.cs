using Prism.Events;

namespace Mebster.Myodam.UI.WPF.Event
{
  public class OpenDetailViewEvent : PubSubEvent<OpenDetailViewEventArgs>
  {
  }

  public class OpenDetailViewEventArgs
  {
    public int Id { get; set; }
    public string ViewModelName { get; set; }
  }
}
