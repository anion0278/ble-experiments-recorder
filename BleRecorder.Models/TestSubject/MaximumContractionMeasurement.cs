namespace BleRecorder.Models.TestSubject;

public class MaximumContractionMeasurement : MeasurementBase
{
    public override MeasurementType Type => MeasurementType.MaximumContraction;

    public SingleContractionRecord Record { get; set; }

    public MaximumContractionMeasurement()
    {
        Record = new SingleContractionRecord() { Data = new List<MeasuredValue>() };
    }
}