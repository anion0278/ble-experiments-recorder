using System.ComponentModel;

namespace BleRecorder.Models.TestSubject;

public enum MeasurementSite
{
    [Description("Left hand")]
    LeftHand,
    [Description("Right hand")]
    RightHand
}