using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubjects;

namespace Mebster.Myodam.Models.Measurements;

public class Measurement
{
    public int Id { get; set; }

    public DateTimeOffset? Date { get; set; }

    public MeasurementType Type { get; set; }

    public PositionDuringMeasurement PositionDuringMeasurement { get; set; }

    public MeasurementSite SiteDuringMeasurement { get; set; }

    [Required]
    [StringLength(40)]
    public string Title { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public int TestSubjectId { get; set; }

    public TestSubject TestSubject { get; set; }

    public DeviceMechanicalAdjustments? AdjustmentsDuringMeasurement { get; set; }

    public StimulationParameters? ParametersDuringMeasurement { get; set; }

    public ICollection<MeasuredValue> ContractionLoadData { get; set; }

    public double MaxContractionLoad => ContractionLoadData.Any() ? ContractionLoadData.Max(v => v.ContractionValue) : 0;

    public Percentage Fatigue => GetFatiguePercentage();

    private Percentage GetFatiguePercentage()
    {
        var max = MaxContractionLoad;
        if (max == 0) return new Percentage(0);

        var min = ContractionLoadData.Any() ? ContractionLoadData.Min(v => v.ContractionValue) : 0;
        return new Percentage((max - min) / max * 100.0, validate: false); // * 100 -> percentage
    }

    public static ICollection<MeasuredValue>? ConvertInternalJsonToForceValues(string json)
    {
        return JsonSerializer.Deserialize<ICollection<MeasuredValue>?>(json);
    }

    public static string ConvertForceValuesToJson(ICollection<MeasuredValue> values)
    {
        return JsonSerializer.Serialize(values);
    }
}

public record MeasuredValue(double ContractionValue, double StimulationCurrent, TimeSpan Timestamp);

public record StatisticsValue(double Value, DateTimeOffset MeasurementDate);