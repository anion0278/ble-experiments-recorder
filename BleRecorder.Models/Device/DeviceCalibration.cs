using System.ComponentModel;
using System.Runtime.Serialization;

namespace BleRecorder.Models.Device;

public class DeviceCalibration
{
    private const double DefaultParametersValue = 0;

    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }
    public double NominalLoad { get; set; }

    public static DeviceCalibration GetDefaultValues() 
    {
        return new DeviceCalibration()
        {
            NoLoadSensorValue = DefaultParametersValue, 
            NominalLoadSensorValue = DefaultParametersValue, 
            NominalLoad = DefaultParametersValue
        };
    }

    public bool IsValid()
    {
        return NominalLoadSensorValue != DefaultParametersValue 
               && NoLoadSensorValue != DefaultParametersValue
               && NominalLoad != DefaultParametersValue;
    }
}