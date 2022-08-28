namespace BleRecorder.Business.Device;

public class MechanicalAdjustmentWithLimits
{
    private double _value;
    public int MinValue { get; }
    public int MaxValue { get; }
    public double Step { get; }

    public double Value
    {
        get => _value;
        set
        {
            if (MinValue > value || value > MaxValue)
                throw new ArgumentException("Mechanical parameter was set out of range!");

            _value = value;
        }
    }

    public MechanicalAdjustmentWithLimits(int minValue, int maxValue, double step, double value)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Step = step;
        Value = value;
    }
}