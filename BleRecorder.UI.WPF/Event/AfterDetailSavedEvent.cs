
namespace BleRecorder.UI.WPF.Event
{

  public class AfterDetailSavedEventArgs
  {
    public int Id { get; set; }
    public string DisplayMember { get; set; }
    public string ViewModelName { get; set; }
  }
}
