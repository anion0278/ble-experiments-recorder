using System.ComponentModel;

namespace Mebster.Myodam.Business.Device;

public enum MyodamError
{
    [Description("No error")]
    NoError = 0,
    [Description("FES connection lost")]
    StimulatorConnectionLost = 1
}