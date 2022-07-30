using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public class MechanismParameters
{
    // Implement Units type? 
    public MechanicalAdjustment AnkleAxisX { get; set; }
    public MechanicalAdjustment AnkleAxisY { get; set; }
    public MechanicalAdjustment AnkleAxisZ { get; set; }
    public MechanicalAdjustment TibiaLength { get; set; }
    public MechanicalAdjustment KneeAxisDeviation { get; set; }

    public MechanismParameters(BleRecorderMechanicalAdjustments adjustments)
    {
        AnkleAxisX = new MechanicalAdjustment(-10, 10, adjustments.AnkleAxisX);
        AnkleAxisY = new MechanicalAdjustment(-10, 10, adjustments.AnkleAxisY);
        AnkleAxisZ = new MechanicalAdjustment(-10, 10, adjustments.AnkleAxisZ);
        TibiaLength = new MechanicalAdjustment(0, 100, adjustments.TibiaLength);
        KneeAxisDeviation = new MechanicalAdjustment(0, 50, adjustments.KneeAxisDeviation);
    }

    public BleRecorderMechanicalAdjustments GetCurrentAdjustments()
    {
        return new BleRecorderMechanicalAdjustments()
        {
            AnkleAxisX = AnkleAxisX.Value,
            AnkleAxisY = AnkleAxisY.Value,
            AnkleAxisZ = AnkleAxisZ.Value,
            TibiaLength = TibiaLength.Value,
            KneeAxisDeviation = KneeAxisDeviation.Value
        };
    }
}