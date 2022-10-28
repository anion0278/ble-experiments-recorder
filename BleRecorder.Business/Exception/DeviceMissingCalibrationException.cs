using BleRecorder.Business.Device;
using BleRecorder.Common.Extensions;

namespace BleRecorder.Business.Exception;

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
    public DeviceErrorOccurredException(BleRecorderError error) : base($"New error on device - {error.GetDescription()}.")
    { }
}