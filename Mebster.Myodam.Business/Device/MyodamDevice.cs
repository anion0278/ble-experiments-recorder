using System.Diagnostics;
using System.Text.RegularExpressions;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class MyodamDevice // TODO Extract inteface
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
            MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public MyodamDevice(IBleDeviceHandler bleDeviceHandler)
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

    public async Task SendMsg(MyodamCommonMessage message)
    {
        await _bleDeviceHandler.Send(message.FormatForSending());
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
    {
        var regex = Regex.Match(data, @"\+(\d+),-?(\d+.\d+)");
        if (regex.Success 
            && int.TryParse(regex.Groups[1].Value, out var timestamp)
            && float.TryParse(regex.Groups[2].Value, out var measuredForceValue) 
            && IsCurrentlyMeasuring) 
        {
            NewValueReceived?.Invoke(
                this, 
                new MeasuredValue(measuredForceValue, TimeSpan.FromMilliseconds(timestamp)));
            return;
        }

        if (data.Contains("Finished"))
        {
            IsCurrentlyMeasuring = false;
            MeasurementFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurement(StimulationParameters parameters)
    {
        await SendMsg(new MyodamCommonMessage(parameters));
        IsCurrentlyMeasuring = true;
    }

    public async Task StopMeasurement()
    {
        var msg = new MyodamCommonMessage(new StimulationParameters(0,0, StimulationPulseWidth.AvailableOptions[1], MeasurementType.MaximumContraction));
        await SendMsg(msg);
        IsCurrentlyMeasuring = false;
    }

    public void Disconnect()
    {
        _bleDeviceHandler.DataReceived -= BleDeviceHandlerDataReceived;
        _bleDeviceHandler.Disconnect();
    }
}