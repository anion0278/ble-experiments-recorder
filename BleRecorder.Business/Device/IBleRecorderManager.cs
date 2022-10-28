using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public interface IBleRecorderManager
{
    event EventHandler BleRecorderAvailabilityChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? DevicePropertyChanged;
    event EventHandler? DeviceErrorChanged;
    IBleRecorderDevice? BleRecorderDevice { get; }
    DeviceCalibration Calibration { get; set; }
    BleRecorderAvailabilityStatus BleRecorderAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectBleRecorderAsync();
    void SetDeviceAddressFilter(ulong? acceptedAddress);
}