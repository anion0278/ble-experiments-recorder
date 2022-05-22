using Prism.Events;

namespace BleRecorder.UI.WPF.Event
{
  public class AfterCollectionSavedEvent : PubSubEvent<AfterCollectionSavedEventArgs>
  {
  }

  public class AfterCollectionSavedEventArgs
  {
    public string ViewModelName { get; set; }
  }
}
