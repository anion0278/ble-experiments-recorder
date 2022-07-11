using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class MyodamReplyMessage
{
    public int Timestamp { get; }
    public float MeasuredForceValue { get; }

    public bool IsMeasurementFinished { get; }

    public MyodamReplyMessage(bool isMeasurementFinished)
    {
        IsMeasurementFinished = isMeasurementFinished;
    }

    public MyodamReplyMessage(int timestamp, float measuredForceValue, bool isMeasurementFinished)
    {
        Timestamp = timestamp;
        MeasuredForceValue = measuredForceValue;
        IsMeasurementFinished = isMeasurementFinished;
    }
}

