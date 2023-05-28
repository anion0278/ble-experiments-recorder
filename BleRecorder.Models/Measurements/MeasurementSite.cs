using System.ComponentModel;

namespace BleRecorder.Models.Measurements;

public enum MeasurementSite
{
    [Description("Left hand")]
    LeftHand,
    [Description("Right hand")]
    RightHand
}