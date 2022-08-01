using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mebster.Myodam.Models.TestSubject;

public enum MeasurementType
{
    [Description("Maximum contraction")]
    MaximumContraction,
    [Description("Fatigue")]
    Fatigue
}