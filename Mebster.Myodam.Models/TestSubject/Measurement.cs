namespace Mebster.Myodam.Models.TestSubject;

public class Measurement
{
    public int Id { get; set; }

    public string Description { get; set; }

    public ICollection<MeasuredValue> ForceData { get; set; }

    public int TestSubjectId { get; set; }
    public TestSubject TestSubject { get; set; }
}

public record MeasuredValue(float Value, DateTime TimeStamp);