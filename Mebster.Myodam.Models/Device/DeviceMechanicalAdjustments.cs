namespace Mebster.Myodam.Models.Device;

public class AdjustableParameter
{
    public int Value { get; set; }
}

public class DeviceMechanicalAdjustments : ICloneable // TODO: Prototype pattern
{
    public int Id { get; private set; }
    public double FootplateAdductionAbductionAngle { get; set; }
    public double FootplateProximalDistalDistance { get; set; }
    public double FootplateAnteroPosteriorDistance { get; set; }
    public double CuffProximalDistalDistance { get; set; }
    public object Clone()
    {
        return new DeviceMechanicalAdjustments()
        {
            FootplateAdductionAbductionAngle = FootplateAdductionAbductionAngle,
            FootplateProximalDistalDistance = FootplateProximalDistalDistance,
            FootplateAnteroPosteriorDistance = FootplateAnteroPosteriorDistance,
            CuffProximalDistalDistance = CuffProximalDistalDistance
        };
    }
}
