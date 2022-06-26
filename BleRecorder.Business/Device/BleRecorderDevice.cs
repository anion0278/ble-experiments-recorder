using System.Diagnostics;
using System.Text.RegularExpressions;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderDevice // TODO Extract inteface
{
    private readonly IBleDeviceHandler _bleDeviceHandler;
    private bool _isCurrentlyMeasuring;
    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? MeasurementFinished;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? MeasurementStatusChanged;

    public bool IsConnected => _bleDeviceHandler.IsConnected;

    public bool IsCurrentlyMeasuring
    {
        get => _isCurrentlyMeasuring;
        private set
        {
            if (_isCurrentlyMeasuring == value) return;
            _isCurrentlyMeasuring = value;
            Debug.Print("Changed meas:" + _isCurrentlyMeasuring);
            MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BleRecorderDevice(IBleDeviceHandler bleDeviceHandler)
    {
        _bleDeviceHandler = bleDeviceHandler;
        _bleDeviceHandler.DataReceived += BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged += BleDeviceStatusChanged;
    }

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        if (!_bleDeviceHandler.IsConnected) IsCurrentlyMeasuring = false;
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
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
        var msg = new BleRecorderCommonMessage(parameters, true);
        await _bleDeviceHandler.Send(msg.FormatForSending());
        IsCurrentlyMeasuring = true;
    }

    public async Task StopMeasurement()
    {
        var msg = new BleRecorderCommonMessage(new StimulationParameters(0,0,0), false);
        await _bleDeviceHandler.Send(msg.FormatForSending());
        IsCurrentlyMeasuring = false;
    }

    public void Disconnect()
    {
        _bleDeviceHandler.DataReceived -= BleDeviceHandlerDataReceived;
        _bleDeviceHandler.Disconnect();
    }
}