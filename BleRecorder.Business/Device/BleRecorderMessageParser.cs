using System.Text.RegularExpressions;

namespace BleRecorder.Business.Device;

public interface IBleRecorderMessageParser
{
    BleRecorderReplyMessage ParseReply(string msg);
}

public class BleRecorderMessageParser : IBleRecorderMessageParser
{
    public BleRecorderReplyMessage ParseReply(string msg)
    {
        var regex = Regex.Match(msg, @"\+(\d+),-?(\d+.\d+)");
        if (regex.Success
            && int.TryParse(regex.Groups[1].Value, out var timestamp)
            && float.TryParse(regex.Groups[2].Value, out var measuredForceValue))
        {
            return new BleRecorderReplyMessage(timestamp, measuredForceValue, false);
        }

        throw new ArgumentException("Invalid bleRecorder message!");
        //if (data.Contains("Finished"))
        //{
        //    IsCurrentlyMeasuring = false;
        //    MeasurementFinished?.Invoke(this, EventArgs.Empty);
        //}

        //return new BleRecorderReplyMessage();
    }
}