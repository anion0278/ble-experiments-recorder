using System.ComponentModel;

namespace BleRecorder.Business.Device;

public enum BleRecorderError
{
    [Description("No error")]
    NoError = 0,
    [Description("Unit connection lost")]
    StimulatorConnectionLost = 1
}