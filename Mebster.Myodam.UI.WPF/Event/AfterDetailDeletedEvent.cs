using Prism.Events;

namespace Mebster.Myodam.UI.WPF.Event
{
  public class AfterDetailDeletedEvent : PubSubEvent<AfterDetailDeletedEventArgs>
  {
  }
  public class AfterDetailDeletedEventArgs
  {
    public int Id { get; set; }
    public string ViewModelName { get; set; }
  }
}
