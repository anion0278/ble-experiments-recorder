namespace Mebster.Myodam.Business.Device;

public class MyodamCommonMessage
{
    public StimulationParameters StimulationParameters { get; }

    public MyodamCommonMessage(StimulationParameters stimulationParameters)
    {
        StimulationParameters = stimulationParameters;
    }

    public string FormatForSending()
    {
        return
            $"C:{StimulationParameters.Current:0.000}mA;" +
            $"F:{StimulationParameters.Frequency:0}Hz;" +
            $"P:{StimulationParameters.PulseWidth:0}us;";
    }

    public override string ToString()
    {
        return FormatForSending();
    }
}