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
        var regex = Regex.Match(msg, @"\+(\d+),-?(\d+.\d+)");
        if (regex.Success
            && int.TryParse(regex.Groups[1].Value, out var timestamp)
            && float.TryParse(regex.Groups[2].Value, out var measuredForceValue))
        {
            return new MyodamReplyMessage(timestamp, measuredForceValue, false);
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