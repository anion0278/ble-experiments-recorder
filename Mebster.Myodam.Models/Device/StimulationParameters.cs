namespace Mebster.Myodam.Models.Device;

public class StimulationParameters : ICloneable
{
    public int Id { get; private set; }

    public int Current { get; set; }
    public int Frequency { get; set; }
    public StimulationPulseWidth PulseWidth { get; set; }
    public TimeSpan StimulationTime { get; set; }
    public TimeSpan FatigueStimulationTime { get; set; }
    public TimeSpan RestTime { get; set; }
    public int FatigueRepetitions { get; set; }


    public StimulationParameters(
        int current, 
        int frequency, 
        StimulationPulseWidth pulseWidth, 
        TimeSpan stimulationTime,
        TimeSpan fatigueStimulationTime,
        TimeSpan restTime, 
        int fatigueRepetitions)
    {
        //if (current < 1 || current > 100) throw new ArgumentException($"Parameter {nameof(Current)} is ")
        // TODO Validate

        Current = current;
        Frequency = frequency;
        PulseWidth = pulseWidth;
        StimulationTime = stimulationTime;
        RestTime = restTime;
        FatigueRepetitions = fatigueRepetitions;
        FatigueStimulationTime = fatigueStimulationTime;
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
            Current,
            Frequency,
            PulseWidth,
            StimulationTime,
            FatigueStimulationTime,
            RestTime,
            FatigueRepetitions
        );
    }
}