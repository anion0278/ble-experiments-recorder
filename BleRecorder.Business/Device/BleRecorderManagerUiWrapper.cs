﻿using BleRecorder.Common.Services;
using BleRecorder.Models.Device;

namespace BleRecorder.Business.Device;

public class BleRecorderManagerUiWrapper : IBleRecorderManager
{
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly ISynchronizationContextProvider _contextProvider;

    public event EventHandler? BleRecorderAvailabilityChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? DevicePropertyChanged;
    public event EventHandler? DeviceErrorChanged;

    public DeviceCalibration Calibration
    {
        get => _bleRecorderManager.Calibration;
        set => _bleRecorderManager.Calibration = value;
    }

    public IBleRecorderDevice? BleRecorderDevice => _bleRecorderManager.BleRecorderDevice;

    public BleRecorderAvailabilityStatus BleRecorderAvailability => _bleRecorderManager.BleRecorderAvailability;
    public bool IsCurrentlyMeasuring => _bleRecorderManager.IsCurrentlyMeasuring;

    public BleRecorderManagerUiWrapper(BleRecorderManager bleRecorderManager, ISynchronizationContextProvider contextProvider)
    {
        _bleRecorderManager = bleRecorderManager;
        _contextProvider = contextProvider;

        _bleRecorderManager.BleRecorderAvailabilityChanged += _bleRecorderManager_BleRecorderAvailabilityChanged; 
        _bleRecorderManager.MeasurementStatusChanged += _bleRecorderManager_MeasurementStatusChanged; 
        _bleRecorderManager.DevicePropertyChanged += _bleRecorderManager_DevicePropertyChanged;
        _bleRecorderManager.DeviceErrorChanged += _bleRecorderManager_DeviceErrorChanged;
    }

    private void _bleRecorderManager_DeviceErrorChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => DeviceErrorChanged?.Invoke(sender, e));
    }

    private void _bleRecorderManager_BleRecorderAvailabilityChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => BleRecorderAvailabilityChanged?.Invoke(sender, e));
    }

    private void _bleRecorderManager_MeasurementStatusChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => MeasurementStatusChanged?.Invoke(sender, e));
    }

    private void _bleRecorderManager_DevicePropertyChanged(object? sender, EventArgs e)
    {
        _contextProvider.RunInContext(() => DevicePropertyChanged?.Invoke(sender, e));
    }

    public Task ConnectBleRecorderAsync()
    {
        return _bleRecorderManager.ConnectBleRecorderAsync();
    }

    public void SetDeviceAddressFilter(ulong? acceptedAddress)
    {
        _bleRecorderManager.SetDeviceAddressFilter(acceptedAddress);
    }
}