using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Extensions;

namespace Mebster.Myodam.Business.Exception;

public class DeviceMissingCalibrationException : System.Exception
{
    public DeviceMissingCalibrationException() : base("Device calibration data are missing. Measurement cannot be started.")
    { }
}

public class DeviceHasErrorException : System.Exception
{
    public DeviceHasErrorException() : base("Fix device errors first. Measurement cannot be started.")
    { }
}

public class DeviceErrorOccurredException : System.Exception
{
    public DeviceErrorOccurredException(MyodamError error) : base($"New error on device - {error.GetDescription()}.")
    { }
}