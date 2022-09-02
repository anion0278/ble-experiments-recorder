namespace Mebster.Myodam.Business.Exception;

public class DeviceCalibrationException : System.Exception
{
    public DeviceCalibrationException() : base("Could not calibrate. Device should be in measuring mode and connected.")
    { }
}