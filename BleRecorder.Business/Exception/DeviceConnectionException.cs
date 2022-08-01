namespace BleRecorder.Business.Exception;

public class DeviceConnectionException : System.Exception
{
    public DeviceConnectionException(System.Exception exception) : base("There was a problem while connecting to device.", exception)
    {}
}