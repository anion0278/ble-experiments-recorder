using Prism.Events;

namespace Mebster.Myodam.UI.WPF.Event
{
  public class AfterDetailClosedEvent : PubSubEvent<AfterDetailClosedEventArgs>
  {
  }
  public class AfterDetailClosedEventArgs
  {
    public int Id { get; set; }
    public string ViewModelName { get; set; }
  }
}
