namespace Mebster.Myodam.Infrastructure.Bluetooth;

public class DeviceHeartbeatTimeoutException : System.Exception
{
    public DeviceHeartbeatTimeoutException() : base("Device stopped responding.")
    { }
}