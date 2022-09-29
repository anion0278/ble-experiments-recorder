namespace Mebster.Myodam.Models.TestSubject;

public class FatigueMeasurement : MeasurementBase
{
    public override MeasurementType Type => MeasurementType.Fatigue;

    public MultipleContractionRecord? MultiCycleRecord { get; set; }

    public FatigueMeasurement()
    {
        MultiCycleRecord = new MultipleContractionRecord() { Data = new List<SingleContractionRecord>() };
    }
}