using System.Drawing;

namespace BleRecorder.Business.Exception;

public class MeasurementIsAlreadyActiveException :System.Exception
{
    public MeasurementIsAlreadyActiveException() :base("Device measurement is already active.")
    { }
}