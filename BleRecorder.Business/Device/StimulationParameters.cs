using System.Transactions;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;


public class StimulationParameters
{
    public int Current { get; set; }
    public int Frequency { get; set; }
    public StimulationPulseWidth PulseWidth { get; set; }
    public TimeSpan StimulationTime { get; set; }

    public StimulationParameters(int current, int frequency, StimulationPulseWidth pulseWidth, TimeSpan stimulationTime)
    {
        //if (current < 1 || current > 100) throw new ArgumentException($"Parameter {nameof(Current)} is ")
        // TODO Validate

        Current = current;
        Frequency = frequency;
        PulseWidth = pulseWidth;
        StimulationTime = stimulationTime;
    }
}
