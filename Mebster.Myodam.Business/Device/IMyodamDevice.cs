using Mebster.Myodam.Models;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.Measurements;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamDevice
{
    event EventHandler<MeasuredValue>? NewValueReceived;
    event EventHandler? ConnectionStatusChanged;
    event EventHandler? ErrorChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? BatteryStatusChanged;
    MyodamError Error { get; }
    DeviceCalibration Calibration { get; set; }
    TimeSpan DataRequestInterval { get; }
    StimulationParameters CurrentParameters { get; set; }
    Percentage StimulatorBattery { get; }
    Percentage ControllerBattery { get; }
    bool IsConnected { get; }

    // state machine would be better
    bool IsCalibrating { get; }

    bool IsCurrentlyMeasuring { get; }
    Task SendMsgAsync(MyodamRequestMessage message);
    Task<double> GetSensorCalibrationValueAsync();
    Task StartMeasurementAsync(StimulationParameters parameters, MeasurementType measurementType);
    Task StopMeasurementAsync();
    Task DisableFesAndDisconnectAsync();
    Task DisconnectAsync();
}