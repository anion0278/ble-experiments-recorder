using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;

namespace Mebster.Myodam.Models.TestSubject;

public enum MeasurementType
{
    MaximumContraction,
    Fatigue
}


public class Measurement
{
    public int Id { get; set; }

    public DateTimeOffset? Date { get; set; }

    public MeasurementType Type { get; set; }

    [Required]
    [StringLength(40)]
    public string Title { get; set; }

    [StringLength(400)]
    public string Notes { get; set; }
    public int TestSubjectId { get; set; }
    public TestSubject TestSubject { get; set; }

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