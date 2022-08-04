namespace Mebster.Myodam.Models.Device;

public class DeviceCalibration
{
    public int id { get; private set; }
    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }
}