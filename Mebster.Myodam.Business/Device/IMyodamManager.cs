using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamAvailabilityChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? DevicePropertyChanged;
    event EventHandler? DeviceErrorChanged;
    IMyodamDevice? MyodamDevice { get; }
    DeviceCalibration Calibration { get; set; }
    MyodamAvailabilityStatus MyodamAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectMyodamAsync();
    void SetDeviceAddressFilter(ulong? acceptedAddress);
}