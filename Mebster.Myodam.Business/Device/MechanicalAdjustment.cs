namespace Mebster.Myodam.Business.Device;

public class MechanicalAdjustment
{
    public int MinValue { get; }
    public int MaxValue { get; }
    public int Value { get; set; }

    public MechanicalAdjustment(int minValue, int maxValue, int value)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Value = value;
    }
}