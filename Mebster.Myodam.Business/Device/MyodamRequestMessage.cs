using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;


public enum MyodamMeasurement // it has to be separated from MeasurementType not only due to UI, but also logically
{
    Idle,
    MaximumContraction,
    Fatigue,
    FatigueIdle
}

public class MyodamRequestMessage
{
    public StimulationParameters StimulationParameters { get; }

    public MyodamMeasurement Measurement { get; }

    public bool IsMeasurementRequested { get; }

    public MyodamRequestMessage(StimulationParameters stimulationParameters, MeasurementType measurementType, bool isMeasurementRequested)
    {
        StimulationParameters = stimulationParameters;
        IsMeasurementRequested = isMeasurementRequested;
        Measurement = IsMeasurementRequested ? Convert(measurementType) : MyodamMeasurement.Idle;
    }

    public string FormatForSending()
    {
        return     
            $">SC:{StimulationParameters.Current:000}_" +
            $"SF:{StimulationParameters.Frequency:000}_" +
            $"SP:{StimulationParameters.PulseWidth.Value:000}_" +
            $"ST:{StimulationParameters.StimulationTime.TotalSeconds:00}_" +
            $"RT:{StimulationParameters.RestTime.TotalSeconds:00}_"+
            $"FR:{StimulationParameters.FatigueRepetitions:00}_"+
            $"MC:{(int)Measurement}\n";
    }

    public static MyodamMeasurement Convert(MeasurementType measurementType)
    {
        return measurementType switch
        {
            MeasurementType.MaximumContraction => MyodamMeasurement.MaximumContraction,
            MeasurementType.Fatigue => MyodamMeasurement.Fatigue,
            _ => throw new ArgumentOutOfRangeException(nameof(measurementType), measurementType, null)
        };
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}