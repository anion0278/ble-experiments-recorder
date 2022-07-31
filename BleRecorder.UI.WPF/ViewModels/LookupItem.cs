namespace BleRecorder.UI.WPF.ViewModels
{
  public class LookupItem
  {
    public int Id { get; set; }

    public string DisplayMember { get; set; }
  }

  public class NullLookupItem : LookupItem // TODO Use to create new test sub
  {
    public new int? Id { get { return null; } }
  }
}
