using System.Text.RegularExpressions;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamMessageParser
{
    MyodamReplyMessage ParseReply(string msg);
}

public class MyodamMessageParser : IMyodamMessageParser
{
    public MyodamReplyMessage ParseReply(string msg)
    { 
        // TODO possible refactoring
        var regex = Regex.Match(msg, @">TS:(?'timestamp'\d{5})_ST:(?'sensor'\d{4})_CB:(?'controller_battery'\d{3})_FB:(?'fes_battery'\d{3})_EC:(?'error'\d)_MS:(?'measurement'\d)");
        if (regex.Success
            && int.TryParse(regex.Groups["timestamp"].Value, out var timestamp)
            && int.TryParse(regex.Groups["sensor"].Value, out var sensorValue)
            && Percentage.TryParse(regex.Groups["controller_battery"].Value, out var controllerBattery)
            && Percentage.TryParse(regex.Groups["fes_battery"].Value, out var stimulatorBattery)
            && Enum.TryParse(typeof(MyodamError), regex.Groups["error"].Value, true, out var errorCode)
            && Enum.TryParse(typeof(MyodamMeasurement), regex.Groups["measurement"].Value, true, out var measurementStatus))
        {
            return new MyodamReplyMessage(
                TimeSpan.FromMilliseconds(timestamp),
                sensorValue / 2.0,
                controllerBattery,
                stimulatorBattery,
                (MyodamError)errorCode!,
                (MyodamMeasurement)measurementStatus!);
        }

        throw new ArgumentException("Invalid myodam message!");
        //if (data.Contains("Finished"))
        //{
        //    IsCurrentlyMeasuring = false;
        //    MeasurementFinished?.Invoke(this, EventArgs.Empty);
        //}

        //return new MyodamReplyMessage();
    }
}