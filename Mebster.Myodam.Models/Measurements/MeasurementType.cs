using System.ComponentModel;

namespace Mebster.Myodam.Models.Measurements;

public enum MeasurementType
{
    [Description("Maximum contraction")]
    MaximumContraction,
    [Description("Fatigue")]
    Fatigue
}