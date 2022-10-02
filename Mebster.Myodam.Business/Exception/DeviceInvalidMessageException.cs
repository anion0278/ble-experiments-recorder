namespace Mebster.Myodam.Business.Exception;

public class DeviceInvalidMessageException : System.Exception
{
    public DeviceInvalidMessageException(string msg) : base($"Device sent invalid message: {msg}")
    { }

    public DeviceInvalidMessageException(string message, System.Exception ex) : base(message, ex)
    { }
}