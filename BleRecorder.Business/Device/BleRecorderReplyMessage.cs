using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderReplyMessage
{
    public int Timestamp { get; }
    public float MeasuredForceValue { get; }

    public bool IsMeasurementFinished { get; }

    public BleRecorderReplyMessage(bool isMeasurementFinished)
    {
        IsMeasurementFinished = isMeasurementFinished;
    }

    public BleRecorderReplyMessage(int timestamp, float measuredForceValue, bool isMeasurementFinished)
    {
        Timestamp = timestamp;
        MeasuredForceValue = measuredForceValue;
        IsMeasurementFinished = isMeasurementFinished;
    }
}

