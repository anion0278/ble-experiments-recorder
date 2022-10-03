using System.Drawing;
using BleRecorder.Models;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderReplyMessage
{
    public TimeSpan Timestamp { get; }
    public double SensorValue { get; }
    public double CurrentMilliAmp { get; }

    public Percentage ControllerBattery { get; }
    public Percentage StimulatorBattery { get; }
    public BleRecorderError Error { get; }
    public BleRecorderMeasurement MeasurementStatus { get; }

    public BleRecorderReplyMessage(
        TimeSpan timestamp, 
        double sensorValue,
        double currentMilliAmp,
        Percentage controllerBattery,
        Percentage stimulatorBattery,
        BleRecorderError error,
        BleRecorderMeasurement measurementStatus)
    {
        Timestamp = timestamp;
        SensorValue = sensorValue;
        CurrentMilliAmp = currentMilliAmp;
        ControllerBattery = controllerBattery;
        StimulatorBattery = stimulatorBattery;
        Error = error;
        MeasurementStatus = measurementStatus;
    }
}

