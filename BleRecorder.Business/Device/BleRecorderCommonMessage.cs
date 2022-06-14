namespace BleRecorder.Business.Device;

public class BleRecorderCommonMessage
{
    public bool IsTorqueMeasurementActive { get; }

    public StimulationParameters StimulationParameters { get; }

    public BleRecorderCommonMessage(StimulationParameters stimulationParameters, bool isTorqueMeasurementActive)
    {
        StimulationParameters = stimulationParameters;
        IsTorqueMeasurementActive = isTorqueMeasurementActive;
    }

    public string FormatForSending()
    {
        return
            $"C:{StimulationParameters.Current:0.000}mA;" +
            $"F:{StimulationParameters.Frequency:0}Hz;" +
            $"P:{StimulationParameters.PulseWidth:0}us;" +
            $"M:{IsTorqueMeasurementActive}";
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}