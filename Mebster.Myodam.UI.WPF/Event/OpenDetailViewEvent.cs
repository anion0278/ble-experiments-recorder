using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Event
{
    public class OpenDetailViewEventArgs
    {
        public int Id { get; init; }
        public string ViewModelName { get; init; }
        public object Data { get; set; }
    }

    //public class CreateMeasurementEventArgs: OpenDetailViewEventArgs
    //{
    //    public TestSubject TestSubject { get; init; }
    //}
}
