namespace Mebster.Myodam.Business.Exception;

public class DeviceMissingCalibrationException : System.Exception
{
    public DeviceMissingCalibrationException() : base("Device calibration data are missing. Cannot start measurement.")
    { }
}