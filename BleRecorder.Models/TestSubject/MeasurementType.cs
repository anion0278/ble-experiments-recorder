using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BleRecorder.Models.TestSubject;

public enum MeasurementType
{
    [Description("Maximum contraction")]
    MaximumContraction,
    [Description("Intermittent")]
    Intermittent
}