namespace BleRecorder.Business.Exception;

public class DeviceConnectionException : System.Exception
{
    public DeviceConnectionException(System.Exception exception) : base("There was a problem while connecting to device.", exception)
    {}
}

public class DeviceCalibrationException : System.Exception
{
    public DeviceCalibrationException() : base("Could not calibrate. Device should be in measuring mode and connected.")
    { }
}

public class DeviceMissingCalibrationException : System.Exception
{
    public DeviceMissingCalibrationException() : base("Device calibration data are missing. Cannot start measurement.")
    { }
}