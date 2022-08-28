using System.ComponentModel;

namespace BleRecorder.Models.Device;

public class DeviceCalibration
{
    private const double DefaultParametersValue = 1;

    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }

    public static DeviceCalibration GetDefaultValues() 
    {
        return new DeviceCalibration() { NoLoadSensorValue = 1, NominalLoadSensorValue = 1};
    }

    public bool IsValid()
    {
        return NominalLoadSensorValue != DefaultParametersValue && NoLoadSensorValue != DefaultParametersValue;
    }
}