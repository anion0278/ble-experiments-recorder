using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public class MechanismParameters
{
    public MechanicalAdjustmentWithLimits FootplateAdductionAbductionAngle { get; }
    public MechanicalAdjustmentWithLimits FootplateProximalDistalDistance { get; }
    public MechanicalAdjustmentWithLimits FootplateAnteroPosteriorDistance { get; }
    public MechanicalAdjustmentWithLimits CuffProximalDistalDistance { get; }

    public MechanismParameters(DeviceMechanicalAdjustments adjustments)
    {
        FootplateAdductionAbductionAngle = new MechanicalAdjustmentWithLimits(-5, 5, 1, adjustments.FootplateAdductionAbductionAngle);
        FootplateAnteroPosteriorDistance = new MechanicalAdjustmentWithLimits(0, 50, 2.5, adjustments.FootplateAnteroPosteriorDistance);
        FootplateProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 20, 2, adjustments.FootplateProximalDistalDistance);
        CuffProximalDistalDistance = new MechanicalAdjustmentWithLimits(0, 100, 10, adjustments.CuffProximalDistalDistance);
    }

    public DeviceMechanicalAdjustments GetCurrentAdjustments()
    {
        return new DeviceMechanicalAdjustments()
        {
            FootplateAdductionAbductionAngle = FootplateAdductionAbductionAngle.Value,
            FootplateProximalDistalDistance = FootplateProximalDistalDistance.Value,
            FootplateAnteroPosteriorDistance = FootplateAnteroPosteriorDistance.Value,
            CuffProximalDistalDistance = CuffProximalDistalDistance.Value,
        };
    }
}