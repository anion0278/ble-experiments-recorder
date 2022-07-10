namespace Mebster.Myodam.UI.WPF.Event
{
    public class AfterDetailClosedEventArgs : IDetailViewEventArgs
    {
        public int Id { get; init; }
        public string ViewModelName { get; init; }
    }
}
