using System.ComponentModel;

namespace BleRecorder.Models.Measurements;

public enum MeasurementType
{
    [Description("Maximum contraction")]
    MaximumContraction,
    [Description("Intermittent")]
    Intermittent
}