namespace BleRecorder.UI.WPF.Event;

public class AfterDetailDeletedEventArgs : IDetailViewEventArgs
{
    public int Id { get; init; }
    public string ViewModelName { get; init; }
}

