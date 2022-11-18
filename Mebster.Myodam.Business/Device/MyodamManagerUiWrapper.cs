using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Business.Device;

public class MyodamManagerUiWrapper : IMyodamManager
{
    private readonly IMyodamManager _myodamManager;
    private readonly ISynchronizationContextProvider _contextProvider;

    public event EventHandler? MyodamAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;
    public event EventHandler? DeviceErrorChanged;

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

        _myodamManager.MyodamAvailabilityChanged += _myodamManager_MyodamAvailabilityChanged; 
        _myodamManager.MeasurementStatusChanged += _myodamManager_MeasurementStatusChanged; 
        _myodamManager.DevicePropertyChanged += _myodamManager_DevicePropertyChanged;
        _myodamManager.DeviceErrorChanged += _myodamManager_DeviceErrorChanged;
    }

    private void _myodamManager_DeviceErrorChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => DeviceErrorChanged?.Invoke(sender, e));
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