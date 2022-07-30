using System.Drawing;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;


public enum BleRecorderError
{
    NoError = 0,
    StimulatorConnectionLost = 1
}

public readonly struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value) 
    {
        Value = value;
        if (value is > 100 or < 0) throw new ArgumentException("Percentage is out of range");
    }

    public static explicit operator Percentage(decimal d)
    {
        return new Percentage(d);
    }

    public static Percentage Parse(string value)
    {
        return new Percentage(decimal.Parse(value));
    }

    public static bool TryParse(string value, out Percentage percentage)
    {
        bool flag = decimal.TryParse(value, out var parsedValue);
        percentage = new Percentage(parsedValue);
        return flag;
    }

    public override string ToString()
    {
        return $"{Value}%";
    }
}

public class BleRecorderReplyMessage
{
    public TimeSpan Timestamp { get; }
    public double SensorValue { get; }
    public Percentage ControllerBattery { get; }
    public Percentage StimulatorBattery { get; }
    public BleRecorderError Error { get; }
    public BleRecorderMeasurement MeasurementStatus { get; }


    public BleRecorderReplyMessage(
        TimeSpan timestamp, 
        double sensorValue,
        Percentage controllerBattery,
        Percentage stimulatorBattery,
        BleRecorderError error,
        BleRecorderMeasurement measurementStatus)
    {
        Timestamp = timestamp;
        SensorValue = sensorValue;
        ControllerBattery = controllerBattery;
        StimulatorBattery = stimulatorBattery;
        Error = error;
        MeasurementStatus = measurementStatus;
    }
}

