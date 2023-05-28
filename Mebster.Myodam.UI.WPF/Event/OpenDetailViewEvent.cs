namespace Mebster.Myodam.UI.WPF.Event
{
    public interface IDetailViewEventArgs
    {
        public int Id { get; init; }
        public string ViewModelName { get; init; }
    }

    public class OpenDetailViewEventArgs : IDetailViewEventArgs
    {
        public int Id { get; init; }
        public string ViewModelName { get; init; }
        public object Data { get; init; }
    }

    //public class CreateMeasurementEventArgs: OpenDetailViewEventArgs
    //{
    //    public TestSubject TestSubject { get; init; }
    //}
}
