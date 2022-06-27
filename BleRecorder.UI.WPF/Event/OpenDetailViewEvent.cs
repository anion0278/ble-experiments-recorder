using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Event
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
