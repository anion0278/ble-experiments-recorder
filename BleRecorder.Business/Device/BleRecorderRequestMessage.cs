using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;

namespace BleRecorder.Business.Device;


public enum BleRecorderCommand // it has to be separated from MeasurementType not only due to UI, but also logically
{
    Idle,
    MaximumContraction,
    Intermittent,
    IntermittentIdle,
    DisableFes
}

public class BleRecorderRequestMessage
{
    public StimulationParameters StimulationParameters { get; }

    public BleRecorderCommand Command { get; }

    public bool IsMeasurementRequested { get; }

    public BleRecorderRequestMessage(StimulationParameters stimulationParameters, MeasurementType measurementType, bool isMeasurementRequested)
    : this(stimulationParameters, isMeasurementRequested ? Convert(measurementType) : BleRecorderCommand.Idle, isMeasurementRequested) 
    { }

    private BleRecorderRequestMessage(StimulationParameters stimulationParameters, BleRecorderCommand command, bool isMeasurementRequested)
    {
        Command = command;
        StimulationParameters = stimulationParameters;
        IsMeasurementRequested = isMeasurementRequested;
    }

    public static BleRecorderRequestMessage GetDisableFesMessage()
    {
        return new BleRecorderRequestMessage(StimulationParameters.GetDefaultValues(), BleRecorderCommand.DisableFes, false);
    }

    public string FormatForSending()
    {
        // TODO Strategy !!!
        var stimTime = Command == BleRecorderCommand.Intermittent 
            ? StimulationParameters.IntermittentStimulationTime.TotalSeconds
            : StimulationParameters.StimulationTime.TotalSeconds;

        // TODO possible refactoring
        return     
            $">SC:{StimulationParameters.Current:000}_" +
            $"SF:{StimulationParameters.Frequency:000}_" +
            $"SP:{StimulationParameters.PulseWidth.Value:000}_" +
            $"ST:{stimTime:00}_" +
            $"RT:{StimulationParameters.RestTime.TotalSeconds:00}_"+
            $"FR:{StimulationParameters.IntermittentRepetitions:00}_"+
            $"MC:{(int)Command}\n";
    }

    public static BleRecorderCommand Convert(MeasurementType measurementType)
    {
        return measurementType switch
        {
            MeasurementType.MaximumContraction => BleRecorderCommand.MaximumContraction,
            MeasurementType.Intermittent => BleRecorderCommand.Intermittent,
            _ => throw new ArgumentOutOfRangeException(nameof(measurementType), measurementType, null)
        };
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}