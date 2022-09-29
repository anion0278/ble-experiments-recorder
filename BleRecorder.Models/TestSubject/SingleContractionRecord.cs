using System.Drawing;
using System.Text.Json;

namespace BleRecorder.Models.TestSubject;

public class SingleContractionRecord
{
    public ICollection<MeasuredValue> Data { get; set; }

    public double MaxContraction => Data.Any() ? Data.Max(v => v.ContractionValue) : 0;

    public SingleContractionRecord()
    {
        Data = new List<MeasuredValue>();
    }
}

public class MultipleContractionRecord
{
    public ICollection<SingleContractionRecord> Data { get; set; }

    // TODO Percentage?
    public double IntermittentPercentage => Data.Any() ? GetIntermittentPercentage() : 0;

    public MultipleContractionRecord()
    {
        Data = new List<SingleContractionRecord>();
    }

    private double GetIntermittentPercentage()
    {
        var data = Data.Select(d => d.MaxContraction).ToArray();
        var max = data.Max();
        var min = data.Min();
        return (max - min) / max;
    }
}

public record MeasuredValue(double ContractionValue, double StimulationCurrent, TimeSpan Timestamp);

public record StatisticsValue(double ContractionForceValue, DateTimeOffset MeasurementDate);