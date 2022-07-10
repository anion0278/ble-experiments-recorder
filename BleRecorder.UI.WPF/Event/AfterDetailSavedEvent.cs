
namespace BleRecorder.UI.WPF.Event
{

    public class AfterDetailSavedEventArgs : IDetailViewEventArgs
    {
        public int Id { get; init; }
        public string DisplayMember { get; init; }
        public string ViewModelName { get; init; }
    }
}
