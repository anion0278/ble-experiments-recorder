﻿using System.Text.RegularExpressions;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderDevice
{
    private readonly IBleAvailableDevice _bleAvailableDevice;
    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? MeasurementFinished;

    private bool IsCurrentlyMeasuring = false; // temp

    public BleRecorderDevice(IBleAvailableDevice bleAvailableDevice)
    {
        _bleAvailableDevice = bleAvailableDevice;
        _bleAvailableDevice.DataReceived += BleAvailableDeviceDataReceived;
    }

    private void BleAvailableDeviceDataReceived(object? sender, string data)
    {
        var regex = Regex.Match(data, @"\+\d+,-?(\d+.\d+)\n");
        if (regex.Success && float.TryParse(regex.Groups[1].Value, out var measuredForceValue) && IsCurrentlyMeasuring) 
        {
            NewValueReceived?.Invoke(this, new MeasuredValue(measuredForceValue, DateTime.Now));
            return;
        }

        if (data.Contains("Finished"))
        {
            MeasurementFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurement(StimulationParameters parameters)
    {
        IsCurrentlyMeasuring = true;
        var msg = new BleRecorderCommonMessage(parameters, true);
        await _bleAvailableDevice.Send(msg.FormatForSending());
    }

    public async Task StopMeasurement()
    {
        IsCurrentlyMeasuring = false;
        var msg = new BleRecorderCommonMessage(new StimulationParameters(0,0,0), false);
        await _bleAvailableDevice.Send(msg.FormatForSending());
    }

    public void Disconnect()
    {
        _bleAvailableDevice.DataReceived -= BleAvailableDeviceDataReceived;
        _bleAvailableDevice.Disconnect();
    }
}