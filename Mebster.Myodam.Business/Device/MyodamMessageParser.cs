using Mebster.Myodam.Business.Exception;
using System.Text.RegularExpressions;
using Mebster.Myodam.Models;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamReplyParser
{
    MyodamReplyMessage ParseReply(string msg);
}

public class MyodamReplyParser : IMyodamReplyParser
{
    public MyodamReplyMessage ParseReply(string msg)
    { 
        // TODO possible refactoring
        var regex = Regex.Match(msg, @">TS:(?'timestamp'\d{5})_ST:(?'sensor'\d{4})_AC:(?'current'\d{5})_CB:(?'controller_battery'\d{3})_FB:(?'fes_battery'\d{3})_EC:(?'error'\d)_MS:(?'measurement'\d)");
        if (regex.Success
            && int.TryParse(regex.Groups["timestamp"].Value, out var timestamp)
            && int.TryParse(regex.Groups["sensor"].Value, out var sensorValue)
            && int.TryParse(regex.Groups["current"].Value, out var currentVal)
            && Percentage.TryParse(regex.Groups["controller_battery"].Value, out var controllerBattery)
            && Percentage.TryParse(regex.Groups["fes_battery"].Value, out var stimulatorBattery)
            && Enum.TryParse(typeof(MyodamError), regex.Groups["error"].Value, true, out var errorCode)
            && Enum.TryParse(typeof(MyodamMeasurement), regex.Groups["measurement"].Value, true, out var measurementStatus))
        {
            return new MyodamReplyMessage(
                TimeSpan.FromMilliseconds(timestamp),
                sensorValue,
                GetCurrentMilliAmp(currentVal), 
                controllerBattery,
                stimulatorBattery,
                (MyodamError)errorCode!,
                (MyodamMeasurement)measurementStatus!);
        }

        throw new DeviceInvalidMessageException(msg);
    }

    private static double GetCurrentMilliAmp(int currentValue)
    {
        return currentValue / 100.0; // We use currentMultiplier=100 to pass current with higher accuracy
    }
}