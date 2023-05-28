using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public class MechanismParameters
{
    public MechanicalAdjustmentWithLimits FixtureAdductionAbductionAngle { get; }
    public MechanicalAdjustmentWithLimits FixtureProximalDistalDistance { get; }
    public MechanicalAdjustmentWithLimits FixtureAnteroPosteriorDistance { get; }
    public MechanicalAdjustmentWithLimits CuffProximalDistalDistance { get; }

    public MechanismParameters(DeviceMechanicalAdjustments adjustments)
    {
        FixtureAdductionAbductionAngle = new MechanicalAdjustmentWithLimits(-5, 5, 1, adjustments.FixtureAdductionAbductionAngle);
        FixtureAnteroPosteriorDistance = new MechanicalAdjustmentWithLimits(0, 10, 1, adjustments.FixtureAnteroPosteriorDistance);
        FixtureProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 2, 1, adjustments.FixtureProximalDistalDistance);
        CuffProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 10, 1, adjustments.CuffProximalDistalDistance);
    }

    public DeviceMechanicalAdjustments GetActiveAdjustments()
    {
        return new DeviceMechanicalAdjustments()
        {
            FixtureAdductionAbductionAngle = FixtureAdductionAbductionAngle.Value,
            FixtureProximalDistalDistance = FixtureProximalDistalDistance.Value,
            FixtureAnteroPosteriorDistance = FixtureAnteroPosteriorDistance.Value,
            CuffProximalDistalDistance = CuffProximalDistalDistance.Value,
        };
    }
}