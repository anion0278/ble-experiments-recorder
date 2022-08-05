namespace BleRecorder.Models.Device;

public class DeviceCalibration
{
    public int Id { get; private set; }
    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }

    public static DeviceCalibration GetDefaultValues(int id = 0) 
    {
        return new DeviceCalibration() { Id = id, NoLoadSensorValue = 1, NominalLoadSensorValue = 1};
    }
}