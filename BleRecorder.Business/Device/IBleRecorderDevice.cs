using BleRecorder.Models;
using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;

namespace BleRecorder.Business.Device;

public interface IBleRecorderDevice
{
    event EventHandler<MeasuredValue>? NewValueReceived;
    event EventHandler? ConnectionStatusChanged;
    event EventHandler? ErrorChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? BatteryStatusChanged;
    BleRecorderError Error { get; }
    DeviceCalibration Calibration { get; set; }
    TimeSpan DataRequestInterval { get; }
    StimulationParameters ActiveParameters { get; set; }
    Percentage UnitBattery { get; }
    Percentage ControllerBattery { get; }
    bool IsConnected { get; }

    // state machine would be better
    bool IsCalibrating { get; }

    bool IsCurrentlyMeasuring { get; }
    Task SendMsgAsync(BleRecorderRequestMessage message);
    Task<double> GetSensorCalibrationValueAsync();
    Task StartMeasurementAsync(StimulationParameters parameters, MeasurementType measurementType);
    Task StopMeasurementAsync();
    Task DisableFesAndDisconnectAsync();
    Task DisconnectAsync();
}