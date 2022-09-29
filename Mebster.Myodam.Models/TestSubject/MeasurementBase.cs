using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Models.TestSubject;

public abstract class MeasurementBase
{
    public int Id { get; set; }

    public DateTimeOffset? Date { get; set; }

    public abstract MeasurementType Type { get; }

    public PositionDuringMeasurement PositionDuringMeasurement { get; set; }

    public MeasurementSite SiteDuringMeasurement { get; set; }

    [Required]
    [StringLength(40)]
    public string Title { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public int TestSubjectId { get; set; }

    public TestSubject TestSubject { get; set; }

    public DeviceMechanicalAdjustments? AdjustmentsDuringMeasurement { get; set; }

    public StimulationParameters? ParametersDuringMeasurement { get; set; }
}
