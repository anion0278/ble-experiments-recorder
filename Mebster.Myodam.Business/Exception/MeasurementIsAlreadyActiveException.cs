using System.Drawing;

namespace Mebster.Myodam.Business.Exception;

public class MeasurementIsAlreadyActiveException :System.Exception
{
    public MeasurementIsAlreadyActiveException() :base("Device measurement is already active.")
    { }
}