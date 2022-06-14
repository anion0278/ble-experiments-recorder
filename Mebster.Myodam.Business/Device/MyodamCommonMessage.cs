namespace Mebster.Myodam.Business.Device;

public class MyodamCommonMessage
{
    public bool IsTorqueMeasurementActive { get; }

    public StimulationParameters StimulationParameters { get; }

    public MyodamCommonMessage(StimulationParameters stimulationParameters, bool isTorqueMeasurementActive)
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