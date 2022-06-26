using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;

namespace Mebster.Myodam.Models.TestSubject;

public class Measurement
{
    private string _internalForceData;
    private ICollection<MeasuredValue> _forceData;

    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string Title { get; set; }

    [MaxLength(400)]
    [Column("Description")]
    public string Notes { get; set; }
    public int TestSubjectId { get; set; }
    public TestSubject TestSubject { get; set; }


    [NotMapped]
    public ICollection<MeasuredValue> ForceData
    {
        get => _forceData;
        set
        {
            _forceData = value;
            _internalForceData = ConvertForceValuesToJson(_forceData);
        }
    }


    // TODO try use  protected property
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string InternalForceData
    {
        get => _internalForceData;
        set
        {
            _internalForceData = value;
            _forceData = ConvertInternalJsonToForceValues(_internalForceData) ?? Array.Empty<MeasuredValue>();
        }
    }

    private static ICollection<MeasuredValue>? ConvertInternalJsonToForceValues(string json)
    {
        return JsonSerializer.Deserialize<ICollection<MeasuredValue>?>(json);
    }

    private static string ConvertForceValuesToJson(ICollection<MeasuredValue> values)
    {
        return JsonSerializer.Serialize(values);
    }
}

public record MeasuredValue(float Value, DateTime TimeStamp);