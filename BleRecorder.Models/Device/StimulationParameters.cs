using BleRecorder.Common;

namespace BleRecorder.Models.Device;

public class StimulationParameters : IEntity, ICloneable
{
    public int Id { get; private set; }

    public int Amplitude { get; set; } 
    public int Frequency { get; set; }
    public virtual StimulationPulseWidth PulseWidth { get; set; }
    public TimeSpan StimulationTime { get; set; }
    public TimeSpan IntermittentStimulationTime { get; set; }
    public TimeSpan RestTime { get; set; }
    public int IntermittentRepetitions { get; set; }

    public StimulationParameters(
        int amplitude, 
        int frequency, 
        StimulationPulseWidth pulseWidth, 
        TimeSpan stimulationTime,
        TimeSpan intermittentStimulationTime,
        TimeSpan restTime, 
        int intermittentRepetitions)
    {
        //if (amplitude < 1 || amplitude > 100) throw new ArgumentException($"Parameter {nameof(Amplitude)} is ")
        // TODO Validate

        Amplitude = amplitude;
        Frequency = frequency;
        PulseWidth = pulseWidth;
        StimulationTime = stimulationTime;
        RestTime = restTime;
        IntermittentRepetitions = intermittentRepetitions;
        IntermittentStimulationTime = intermittentStimulationTime;
    }

    public static StimulationParameters GetDefaultValues(int id = 0) // TODO into  DefaultValuesFactory
    {
        return new StimulationParameters(
            10,
            50,
            StimulationPulseWidth.AvailableOptions[0],
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            4
            )
        { Id = id };
    }

    public object Clone()
    {
        return new StimulationParameters(
            Amplitude,
            Frequency,
            PulseWidth,
            StimulationTime,
            IntermittentStimulationTime,
            RestTime,
            IntermittentRepetitions
        );
    }
}