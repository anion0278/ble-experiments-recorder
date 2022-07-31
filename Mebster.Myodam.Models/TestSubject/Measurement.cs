using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Models.TestSubject;

public class Measurement
{
    public int Id { get; set; }

    public DateTimeOffset? Date { get; set; }

    public MeasurementType Type { get; set; }

    public MeasurementPosition PositionDuringMeasurement { get; set; }

    public MeasurementSite SiteDuringMeasurement { get; set; }

    [Required]
    [StringLength(40)]
    public string Title { get; set; }


    [StringLength(400)]
    public string? Notes { get; set; }

    public int TestSubjectId { get; set; }

    public TestSubject TestSubject { get; set; }

    public DeviceMechanicalAdjustments AdjustmentsDuringMeasurement { get; set; }

    public StimulationParameters ParametersDuringMeasurement { get; set; }

    public ICollection<MeasuredValue> ForceData { get; set; }

    public static ICollection<MeasuredValue>? ConvertInternalJsonToForceValues(string json)
    {
        return JsonSerializer.Deserialize<ICollection<MeasuredValue>?>(json);
    }

    public static string ConvertForceValuesToJson(ICollection<MeasuredValue> values)
    {
        return JsonSerializer.Serialize(values);
    }
}

public record MeasuredValue(double Value, TimeSpan Timestamp);