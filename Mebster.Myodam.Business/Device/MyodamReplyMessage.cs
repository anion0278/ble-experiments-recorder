﻿using System.Drawing;
using Mebster.Myodam.Models;

namespace Mebster.Myodam.Business.Device;

public class MyodamReplyMessage
{
    public TimeSpan Timestamp { get; }
    public double SensorValue { get; }
    public double CurrentMilliAmp { get; }

    public Percentage ControllerBattery { get; }
    public Percentage StimulatorBattery { get; }
    public MyodamError Error { get; }
    public MyodamCommand CommandStatus { get; }

    public MyodamReplyMessage(
        TimeSpan timestamp, 
        double sensorValue,
        double currentMilliAmp,
        Percentage controllerBattery,
        Percentage stimulatorBattery,
        MyodamError error,
        MyodamCommand commandStatus)
    {
        Timestamp = timestamp;
        SensorValue = sensorValue;
        CurrentMilliAmp = currentMilliAmp;
        ControllerBattery = controllerBattery;
        StimulatorBattery = stimulatorBattery;
        Error = error;
        CommandStatus = commandStatus;
    }
}

