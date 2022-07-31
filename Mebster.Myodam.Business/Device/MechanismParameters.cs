using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public class MechanismParameters
{
    public MechanicalAdjustmentWithLimits AnkleAxisX { get; }
    public MechanicalAdjustmentWithLimits AnkleAxisY { get; }
    public MechanicalAdjustmentWithLimits AnkleAxisZ { get; }
    public MechanicalAdjustmentWithLimits TibiaLength { get; }
    public MechanicalAdjustmentWithLimits KneeAxisDeviation { get; }

    public MechanismParameters(DeviceMechanicalAdjustments adjustments)
    {
        AnkleAxisX = new MechanicalAdjustmentWithLimits(-10, 10, 2, adjustments.AnkleAxisX);
        AnkleAxisY = new MechanicalAdjustmentWithLimits(-10, 10, 2, adjustments.AnkleAxisY);
        AnkleAxisZ = new MechanicalAdjustmentWithLimits(-10, 10, 5, adjustments.AnkleAxisZ);
        TibiaLength = new MechanicalAdjustmentWithLimits(0, 100, 10, adjustments.TibiaLength);
        KneeAxisDeviation = new MechanicalAdjustmentWithLimits(0, 50, 5, adjustments.KneeAxisDeviation);
    }

    public DeviceMechanicalAdjustments GetCurrentAdjustments()
    {
        return new DeviceMechanicalAdjustments()
        {
            AnkleAxisX = AnkleAxisX.Value,
            AnkleAxisY = AnkleAxisY.Value,
            AnkleAxisZ = AnkleAxisZ.Value,
            TibiaLength = TibiaLength.Value,
            KneeAxisDeviation = KneeAxisDeviation.Value
        };
    }
}