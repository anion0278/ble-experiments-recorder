using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace Mebster.Myodam.Models.Device;

public class DeviceCalibration
{
    private LinearEquation _linearEquation;
    private const double DefaultParametersValue = 0;
    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }
    public double NominalLoad { get; set; }

    public DeviceCalibration()
    {
    }

    public void UpdateCalibration()
    {
        if (!IsValid()) throw new ArgumentException($"Calibration is invalid.");

        _linearEquation = new LinearEquation(
            new CalibrationData(NoLoadSensorValue, 0),
            new CalibrationData(NominalLoadSensorValue, NominalLoad));
    }

    public static DeviceCalibration GetDefaultValues() 
    {
        return new DeviceCalibration()
        {
            NoLoadSensorValue = DefaultParametersValue, 
            NominalLoadSensorValue = DefaultParametersValue, 
            NominalLoad = DefaultParametersValue
        };
    }

    public double CalculateLoadValue(double sensorValue)
    {
        return _linearEquation.CalculateLoadValue(sensorValue);
    }

    public bool IsValid()
    {
        return NominalLoadSensorValue != DefaultParametersValue 
               && NoLoadSensorValue != DefaultParametersValue
               && NominalLoad != DefaultParametersValue

               && NominalLoadSensorValue > NoLoadSensorValue; 
    }
}

