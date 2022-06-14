using System.Transactions;

namespace Mebster.Myodam.Business.Device;

public class StimulationParameters
{
    public float Current { get; } // TODO get rid of "primitive obsession" !
    public float Frequency { get; }
    public float PulseWidth { get; }

    public StimulationParameters(float current, float frequency, float pulseWidth)
    {
        Current = current;
        Frequency = frequency;
        PulseWidth = pulseWidth;
    }
}