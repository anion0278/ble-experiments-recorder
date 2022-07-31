namespace Mebster.Myodam.Business.Device;

public class MechanicalAdjustmentWithLimits
{
    private int _value;
    public int MinValue { get; }
    public int MaxValue { get; }
    public int Step { get; }

    public int Value
    {
        get => _value;
        set
        {
            if (MinValue > value || value > MaxValue)
                throw new ArgumentException("Mechanical parameter was set out of range!");

            _value = value;
        }
    }

    public MechanicalAdjustmentWithLimits(int minValue, int maxValue, int step, int value)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Step = step;
        Value = value;
    }
}