using BleRecorder.Common;

namespace BleRecorder.Models.Device;

public class AdjustableParameter
{
    public int Value { get; set; }
}

public class DeviceMechanicalAdjustments : IEntity, ICloneable // TODO: Prototype pattern
{
    public int Id { get; private set; }
    public double FixtureAdductionAbductionAngle { get; set; }
    public double FixtureProximalDistalDistance { get; set; }
    public double FixtureAnteroPosteriorDistance { get; set; }
    public double CuffProximalDistalDistance { get; set; }
    public object Clone()
    {
        return new DeviceMechanicalAdjustments()
        {
            FixtureAdductionAbductionAngle = FixtureAdductionAbductionAngle,
            FixtureProximalDistalDistance = FixtureProximalDistalDistance,
            FixtureAnteroPosteriorDistance = FixtureAnteroPosteriorDistance,
            CuffProximalDistalDistance = CuffProximalDistalDistance
        };
    }
}
