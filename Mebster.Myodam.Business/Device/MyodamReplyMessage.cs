using System.Drawing;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class MyodamReplyMessage
{
    public TimeSpan Timestamp { get; }
    public double SensorValue { get; }
    public Percentage ControllerBattery { get; }
    public Percentage StimulatorBattery { get; }
    public MyodamError Error { get; }
    public MyodamMeasurement MeasurementStatus { get; }

    public MyodamReplyMessage(
        TimeSpan timestamp, 
        double sensorValue,
        Percentage controllerBattery,
        Percentage stimulatorBattery,
        MyodamError error,
        MyodamMeasurement measurementStatus)
    {
        Timestamp = timestamp;
        SensorValue = sensorValue;
        ControllerBattery = controllerBattery;
        StimulatorBattery = stimulatorBattery;
        Error = error;
        MeasurementStatus = measurementStatus;
    }
}

