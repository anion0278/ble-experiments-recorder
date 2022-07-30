using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.Business.Device;

public class MyodamDevice // TODO Extract inteface
{
    private readonly MyodamManager _myodamManager;
    private readonly IBleDeviceHandler _bleDeviceHandler;
    private readonly IMyodamMessageParser _messageParser;
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

    public MyodamDevice(MyodamManager myodamManager, IBleDeviceHandler bleDeviceHandler,
        IMyodamMessageParser messageParser)
    {
        _myodamManager = myodamManager;
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
                new(0, 0, StimulationPulseWidth.AvailableOptions[0], TimeSpan.Zero),
                MeasurementType.Fatigue,
                false));
        }
    }

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        if (!_bleDeviceHandler.IsConnected) IsCurrentlyMeasuring = false;
    }

    public async Task SendMsg(MyodamRequestMessage message)
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
        await SendMsg(new MyodamRequestMessage(_myodamManager.CurrentStimulationParameters, 
            MeasurementType.MaximumContraction, 
            true));
        IsCurrentlyMeasuring = true;
    }

    public async Task StopMeasurement()
    {
        var msg = new MyodamRequestMessage(_myodamManager.CurrentStimulationParameters, 
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