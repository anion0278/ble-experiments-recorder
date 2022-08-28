namespace BleRecorder.Models.Device;

public class AdjustableParameter
{
    public int Value { get; set; }
}

public class DeviceMechanicalAdjustments : ICloneable
{
    public int Id { get; private set; }
    public double FixtureAdductionAbductionAngle { get; set; }
    public double FixtureProximalDistalDistance { get; set; }
    public double FixtureAnteroPosteriorDistance { get; set; }
    public double CuffProximalDistalDistance { get; set; }
    public object Clone()
    {
        return MemberwiseClone();
    }
}