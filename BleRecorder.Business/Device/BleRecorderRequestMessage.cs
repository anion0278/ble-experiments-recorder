using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;


public enum BleRecorderMeasurement // it has to be separated from MeasurementType not only due to UI, but also logically
{
    Idle,
    MaximumContraction,
    Intermittent
}

public class BleRecorderRequestMessage
{
    public StimulationParameters StimulationParameters { get; }

    public BleRecorderMeasurement Measurement { get; }

    public bool IsMeasurementRequested { get; }

    public BleRecorderRequestMessage(StimulationParameters stimulationParameters, MeasurementType measurementType, bool isMeasurementRequested)
    {
        StimulationParameters = stimulationParameters;
        IsMeasurementRequested = isMeasurementRequested;
        Measurement = IsMeasurementRequested ? Convert(measurementType) : BleRecorderMeasurement.Idle;
    }

    public string FormatForSending()
    {
        return     
            $">SC:{StimulationParameters.Current:000}_" +
            $"SF:{StimulationParameters.Frequency:000}_" +
            $"SP:{StimulationParameters.PulseWidth.Value:000}_" +
            $"ST:{StimulationParameters.StimulationTime.TotalSeconds:00}_" +
            $"MC:{(int)Measurement}";
    }

    public static BleRecorderMeasurement Convert(MeasurementType measurementType)
    {
        return measurementType switch
        {
            MeasurementType.MaximumContraction => BleRecorderMeasurement.MaximumContraction,
            MeasurementType.Intermittent => BleRecorderMeasurement.Intermittent,
            _ => throw new ArgumentOutOfRangeException(nameof(measurementType), measurementType, null)
        };
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}