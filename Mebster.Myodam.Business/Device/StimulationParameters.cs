using System.Transactions;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class StimulationParameters
{
    public float Current { get; } // TODO get rid of "primitive obsession" !
    public float Frequency { get; }
    public StimulationPulseWidth PulseWidth { get; }

    public MeasurementType MeasurementType { get; }

    public StimulationParameters(float current, float frequency, StimulationPulseWidth pulseWidth, MeasurementType measurementType)
    {
        //if (current < 1 || current > 100) throw new ArgumentException($"Parameter {nameof(Current)} is ")
        // TODO Validate

        Current = current;
        Frequency = frequency;
        PulseWidth = pulseWidth;
        MeasurementType = measurementType;
    }
}

public class StimulationPulseWidth
{
    public int PulseWidth { get; }

    private StimulationPulseWidth(int pulseWidth)
    {
        PulseWidth = pulseWidth;
    }

    static StimulationPulseWidth()
    {
        AvailableOptions = new[] { 50, 100, 200, 300, 400 }.Select(v => new StimulationPulseWidth(v)).ToArray();
    }

    public static StimulationPulseWidth[] AvailableOptions { get; }
}