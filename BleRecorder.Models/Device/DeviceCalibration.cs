using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace BleRecorder.Models.Device;

public class DeviceCalibration
{
    private LinearEquation _linearEquation;
    private const double DefaultParametersValue = 0;
    public double NoLoadSensorValue { get; set; }
    public double NominalLoadSensorValue { get; set; }
    public double NominalLoadNewtonMeters { get; set; }
    public double LeverLengthMeters { get; set; }

    public void UpdateCalibration()
    {
        if (!IsValid()) throw new ArgumentException($"Calibration is invalid.");

        _linearEquation = new LinearEquation(
            new CalibrationData(NoLoadSensorValue, 0),
            new CalibrationData(NominalLoadSensorValue, NominalLoadNewtonMeters));
    }

    public static DeviceCalibration GetDefaultValues() 
    {
        return new DeviceCalibration()
        {
            NoLoadSensorValue = DefaultParametersValue, 
            NominalLoadSensorValue = DefaultParametersValue, 
            NominalLoadNewtonMeters = DefaultParametersValue
        };
    }

    public double CalculateLoadValue(double sensorValue)
    {
        return _linearEquation.CalculateYValue(sensorValue) / LeverLengthMeters;
    }

    public bool IsValid()
    {
        return NominalLoadSensorValue != DefaultParametersValue 
               && NoLoadSensorValue != DefaultParametersValue
               && NominalLoadNewtonMeters != DefaultParametersValue
               && LeverLengthMeters != DefaultParametersValue
               && NominalLoadSensorValue > NoLoadSensorValue; 
    }
}

