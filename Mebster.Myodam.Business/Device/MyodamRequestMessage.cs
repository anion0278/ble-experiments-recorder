using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.Measurements;

namespace Mebster.Myodam.Business.Device;


public enum MyodamCommand // it has to be separated from MeasurementType not only due to UI, but also logically
{
    Idle,
    MaximumContraction,
    Fatigue,
    FatigueIdle,
    DisableFes
}

public class MyodamRequestMessage
{
    public StimulationParameters StimulationParameters { get; }

    public MyodamCommand Command { get; }

    public bool IsMeasurementRequested { get; }

    public MyodamRequestMessage(StimulationParameters stimulationParameters, MeasurementType measurementType, bool isMeasurementRequested)
    : this(stimulationParameters, isMeasurementRequested ? Convert(measurementType) : MyodamCommand.Idle, isMeasurementRequested) 
    { }

    private MyodamRequestMessage(StimulationParameters stimulationParameters, MyodamCommand command, bool isMeasurementRequested)
    {
        Command = command;
        StimulationParameters = stimulationParameters;
        IsMeasurementRequested = isMeasurementRequested;
    }

    public static MyodamRequestMessage GetDisableFesMessage()
    {
        return new MyodamRequestMessage(StimulationParameters.GetDefaultValues(), MyodamCommand.DisableFes, false);
    }

    public string FormatForSending()
    {
        // TODO Strategy !!!
        var stimTime = Command == MyodamCommand.Fatigue 
            ? StimulationParameters.FatigueStimulationTime.TotalSeconds
            : StimulationParameters.StimulationTime.TotalSeconds;

        // TODO possible refactoring
        return     
            $">SC:{StimulationParameters.Current:000}_" +
            $"SF:{StimulationParameters.Frequency:000}_" +
            $"SP:{StimulationParameters.PulseWidth.Value:000}_" +
            $"ST:{stimTime:00}_" +
            $"RT:{StimulationParameters.RestTime.TotalSeconds:00}_"+
            $"FR:{StimulationParameters.FatigueRepetitions:00}_"+
            $"MC:{(int)Command}\n";
    }

    public static MyodamCommand Convert(MeasurementType measurementType)
    {
        return measurementType switch
        {
            MeasurementType.MaximumContraction => MyodamCommand.MaximumContraction,
            MeasurementType.Fatigue => MyodamCommand.Fatigue,
            _ => throw new ArgumentOutOfRangeException(nameof(measurementType), measurementType, null)
        };
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}