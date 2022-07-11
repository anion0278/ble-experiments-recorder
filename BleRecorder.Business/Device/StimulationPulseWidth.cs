namespace BleRecorder.Business.Device;

public class StimulationPulseWidth
{
    public int Value { get; }
    public static StimulationPulseWidth[] AvailableOptions { get; }

    private StimulationPulseWidth(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    static StimulationPulseWidth()
    {
        AvailableOptions = new[] { 50, 100, 200, 300, 400 }.Select(v => new StimulationPulseWidth(v)).ToArray();
    }
}