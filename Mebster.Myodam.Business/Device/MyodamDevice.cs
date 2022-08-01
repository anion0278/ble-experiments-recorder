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
    private Percentage _stimulatorBattery;
    private Percentage _controllerBattery;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? MeasurementFinished;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;

    public StimulationParameters CurrentParameters { get; set; }

    public MechanismParameters MechanismParameters { get; set; }

    public Percentage StimulatorBattery
    {
        get => _stimulatorBattery;
        private set
        {
            if (_stimulatorBattery.Equals(value)) return;
            _stimulatorBattery = value;
            BatteryStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Percentage ControllerBattery
    {
        get => _controllerBattery;
        private set
        {
            if (_controllerBattery.Equals(value)) return;
            _controllerBattery = value;
            BatteryStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public MyodamDevice(MyodamManager myodamManager, IBleDeviceHandler bleDeviceHandler, IMyodamMessageParser messageParser)
    {
        _myodamManager = myodamManager;
        _bleDeviceHandler = bleDeviceHandler;
        _messageParser = messageParser;
        _bleDeviceHandler.DataReceived += BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged += BleDeviceStatusChanged;

        _outboundDataTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100).TotalMilliseconds);
        _outboundDataTimer.Elapsed += OnTimerTimeElapsed;
        _outboundDataTimer.Start();

        StimulatorBattery = new Percentage(0);
        ControllerBattery = new Percentage(0);
    }

    private async void OnTimerTimeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsConnected) return;

        //Debug.Print("Sent");
        await SendMsg(new(
            StimulationParameters.GetDefaultValues(), 
            MeasurementType.Fatigue,
            false));
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
        //Debug.Print("Recieved");
        var reply = _messageParser.ParseReply(data);
        StimulatorBattery = reply.StimulatorBattery;
        ControllerBattery = reply.ControllerBattery;

        if (IsCurrentlyMeasuring)
        {
            NewValueReceived?.Invoke(
                this,
                new MeasuredValue(reply.SensorValue, reply.Timestamp));
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurement(StimulationParameters parameters)
    {
        CurrentParameters = parameters;
        await SendMsg(new MyodamRequestMessage(
            CurrentParameters, 
            MeasurementType.MaximumContraction, 
            true));
        IsCurrentlyMeasuring = true;
    }

    public async Task StopMeasurement()
    {
        var msg = new MyodamRequestMessage(
            CurrentParameters, 
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