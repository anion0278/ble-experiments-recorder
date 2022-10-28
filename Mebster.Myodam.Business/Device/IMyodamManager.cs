using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public interface IMyodamManager
{
    event EventHandler MyodamAvailabilityChanged;
    event EventHandler? MeasurementStatusChanged;
    event EventHandler? DevicePropertyChanged;
    IMyodamDevice? MyodamDevice { get; }
    DeviceCalibration Calibration { get; set; }
    MyodamAvailabilityStatus MyodamAvailability { get; }
    bool IsCurrentlyMeasuring { get; }
    Task ConnectMyodamAsync();
    void SetDeviceAddressFilter(ulong? acceptedAddress);
}

public class MyodamManagerUiWrapper : IMyodamManager
{
    private readonly IMyodamManager _myodamManager;
    private readonly ISynchronizationContextProvider _contextProvider;

    public event EventHandler? MyodamAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;

    public DeviceCalibration Calibration
    {
        get => _myodamManager.Calibration;
        set => _myodamManager.Calibration = value;
    }

    public IMyodamDevice? MyodamDevice => _myodamManager.MyodamDevice;

    public MyodamAvailabilityStatus MyodamAvailability => _myodamManager.MyodamAvailability;
    public bool IsCurrentlyMeasuring => _myodamManager.IsCurrentlyMeasuring;

    public MyodamManagerUiWrapper(MyodamManager myodamManager, ISynchronizationContextProvider contextProvider)
    {
        _myodamManager = myodamManager;
        _contextProvider = contextProvider;

        _myodamManager.MyodamAvailabilityChanged += _myodamManager_MyodamAvailabilityChanged; ;
        _myodamManager.MeasurementStatusChanged += _myodamManager_MeasurementStatusChanged; ;
        _myodamManager.DevicePropertyChanged += _myodamManager_DevicePropertyChanged; ;
    }

    private void _myodamManager_MyodamAvailabilityChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => MyodamAvailabilityChanged?.Invoke(sender, e));
    }

    private void _myodamManager_MeasurementStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => MeasurementStatusChanged?.Invoke(sender, e));
    }

    private void _myodamManager_DevicePropertyChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => DevicePropertyChanged?.Invoke(sender, e));
    }

    public Task ConnectMyodamAsync()
    {
        return _myodamManager.ConnectMyodamAsync();
    }

    public void SetDeviceAddressFilter(ulong? acceptedAddress)
    {
        _myodamManager.SetDeviceAddressFilter(acceptedAddress);
    }
}