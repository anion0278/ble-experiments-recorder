namespace Mebster.Myodam.Models.Device;

public class AdjustableParameter
{
    public int Value { get; set; }
}

public class DeviceMechanicalAdjustments : ICloneable
{
    public int Id { get; private set; }
    public double FootplateAdductionAbductionAngle { get; set; }
    public double FootplateProximalDistalDistance { get; set; }
    public double FootplateAnteroPosteriorDistance { get; set; }
    public double CuffProximalDistalDistance { get; set; }
    public object Clone()
    {
        return MemberwiseClone();
    }
}