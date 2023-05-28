using System.ComponentModel;

namespace BleRecorder.Models.Measurements;

public enum PositionDuringMeasurement
{
    [Description("Seated")]
    Seated,
    [Description("Standing")]
    Standing
}