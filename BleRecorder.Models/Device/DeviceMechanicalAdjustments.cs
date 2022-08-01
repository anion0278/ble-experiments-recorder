namespace BleRecorder.Models.Device;

public class AdjustableParameter
{
    public int Value { get; set; }
}

public class DeviceMechanicalAdjustments : ICloneable
{
    public int Id { get; private set; }
    public int AnkleAxisX { get; set; }
    public int AnkleAxisY { get; set; }
    public int AnkleAxisZ { get; set; }
    public int TibiaLength { get; set; }
    public int KneeAxisDeviation { get; set; }
    public object Clone()
    {
        return MemberwiseClone();
    }
}