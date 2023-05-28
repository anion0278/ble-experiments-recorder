using BleRecorder.Business.Exception;
using System.Text.RegularExpressions;
using BleRecorder.Models;

namespace BleRecorder.Business.Device;

public interface IBleRecorderReplyParser
{
    BleRecorderReplyMessage ParseReply(string msg);
}

public class BleRecorderReplyParser : IBleRecorderReplyParser
{
    public BleRecorderReplyMessage ParseReply(string msg)
    { 
        // TODO possible refactoring
        var regex = Regex.Match(msg, @">TS:(?'timestamp'\d{5})_ST:(?'sensor'\d{4})_AC:(?'current'\d{5})_CB:(?'controller_battery'\d{3})_FB:(?'fes_battery'\d{3})_EC:(?'error'\d)_MS:(?'measurement'\d)");
        if (regex.Success
            && int.TryParse(regex.Groups["timestamp"].Value, out var timestamp)
            && int.TryParse(regex.Groups["sensor"].Value, out var sensorValue)
            && int.TryParse(regex.Groups["current"].Value, out var amplitudeVal)
            && Percentage.TryParse(regex.Groups["controller_battery"].Value, out var controllerBattery)
            && Percentage.TryParse(regex.Groups["fes_battery"].Value, out var stimulatorBattery)
            && Enum.TryParse(typeof(BleRecorderError), regex.Groups["error"].Value, true, out var errorCode)
            && Enum.TryParse(typeof(BleRecorderCommand), regex.Groups["measurement"].Value, true, out var measurementStatus))
        {
            return new BleRecorderReplyMessage(
                TimeSpan.FromMilliseconds(timestamp),
                sensorValue,
                GetAmplitude(amplitudeVal), 
                controllerBattery,
                stimulatorBattery,
                (BleRecorderError)errorCode!,
                (BleRecorderCommand)measurementStatus!);
        }

        throw new DeviceInvalidMessageException(msg);
    }

    private static double GetAmplitude(int currentValue)
    {
        return currentValue / 100.0; // We use Multiplier=100 to pass value with higher accuracy
    }
}