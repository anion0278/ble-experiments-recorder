using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class BleRecorderDevice // TODO Extract inteface
{
    private readonly BleRecorderManager _bleRecorderManager;
    private readonly IBleDeviceHandler _bleDeviceHandler;
    private readonly IBleRecorderMessageParser _messageParser;
    private bool _isCurrentlyMeasuring;
    private System.Timers.Timer _outboundDataTimer;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? MeasurementFinished;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? MeasurementStatusChanged;

    public StimulationParameters CurrentParameters { get; set; }

    public MechanismParameters Mechanism { get; set; }

    public bool IsConnected => _bleDeviceHandler.IsConnected;

    public bool IsCurrentlyMeasuring
    {
        get => _isCurrentlyMeasuring;
        private set
        {
            if (_isCurrentlyMeasuring && value == false)
            {
                MeasurementFinished?.Invoke(this, EventArgs.Empty);
            }

            if (_isCurrentlyMeasuring == value) return;

            _isCurrentlyMeasuring = value;
            MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BleRecorderDevice(BleRecorderManager bleRecorderManager, IBleDeviceHandler bleDeviceHandler,
        IBleRecorderMessageParser messageParser)
    {
        _bleRecorderManager = bleRecorderManager;
        _bleDeviceHandler = bleDeviceHandler;
        _messageParser = messageParser;
        _bleDeviceHandler.DataReceived += BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged += BleDeviceStatusChanged;

        _outboundDataTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100).TotalMilliseconds);
        _outboundDataTimer.Elapsed += OnTimerTimeElapsed;
        _outboundDataTimer.Start();
    }

    private void OnTimerTimeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (IsConnected)
        {
            Debug.Print("Sent");
            SendMsg(new(
                StimulationParameters.GetDefaultValues(), 
                MeasurementType.Intermittent,
                false));
        }
    }

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        if (!_bleDeviceHandler.IsConnected) IsCurrentlyMeasuring = false;
    }

    public async Task SendMsg(BleRecorderRequestMessage message)
    {
        await _bleDeviceHandler.Send(message.FormatForSending());
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
    {
        var reply = _messageParser.ParseReply(data);

        if (IsCurrentlyMeasuring)
        {
            NewValueReceived?.Invoke(
                this,
                new MeasuredValue(reply.SensorValue, reply.Timestamp));
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurement()
    {
        await SendMsg(new BleRecorderRequestMessage(_bleRecorderManager.CurrentStimulationParameters, 
            MeasurementType.MaximumContraction, 
            true));
        IsCurrentlyMeasuring = true;
    }

    public async Task StopMeasurement()
    {
        var msg = new BleRecorderRequestMessage(_bleRecorderManager.CurrentStimulationParameters, 
            MeasurementType.MaximumContraction,
            false);
        await SendMsg(msg);
        IsCurrentlyMeasuring = false;
    }

    public void Disconnect()
    {
        _bleDeviceHandler.DataReceived -= BleDeviceHandlerDataReceived;
        _bleDeviceHandler.Disconnect();
    }
}