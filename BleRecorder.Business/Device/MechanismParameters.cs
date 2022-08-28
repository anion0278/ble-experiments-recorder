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
        FixtureAnteroPosteriorDistance = new MechanicalAdjustmentWithLimits(0, 50, 2.5, adjustments.FixtureAnteroPosteriorDistance);
        FixtureProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 20, 2, adjustments.FixtureProximalDistalDistance);
        CuffProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 100, 10, adjustments.CuffProximalDistalDistance);
    }

    public DeviceMechanicalAdjustments GetCurrentAdjustments()
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