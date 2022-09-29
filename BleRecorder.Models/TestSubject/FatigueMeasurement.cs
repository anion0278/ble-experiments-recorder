namespace BleRecorder.Models.TestSubject;

public class IntermittentMeasurement : MeasurementBase
{
    public override MeasurementType Type => MeasurementType.Intermittent;

    public MultipleContractionRecord? MultiCycleRecord { get; set; }

    public IntermittentMeasurement()
    {
        MultiCycleRecord = new MultipleContractionRecord() { Data = new List<SingleContractionRecord>() };
    }
}