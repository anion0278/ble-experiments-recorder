using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using Mebster.Myodam.Business.Exception;
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
    private StimulationParameters? _currentParameters;
    private long? _firstTimeStamp; // TODO remove

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;

    public TimeSpan DataRequestInterval { get; } = TimeSpan.FromMilliseconds(100);

    public StimulationParameters CurrentParameters
    {
        get => _currentParameters ?? StimulationParameters.GetDefaultValues();
        set => _currentParameters = value;
    }

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

    public bool IsCalibrating { get; private set; }

    public bool IsCurrentlyMeasuring
    {
        get => _isCurrentlyMeasuring;
        private set
        {
            if (_isCurrentlyMeasuring == value) return;

            _isCurrentlyMeasuring = value;

            if (!value) _firstTimeStamp = null;
            else _firstTimeStamp ??= DateTimeOffset.Now.ToUnixTimeMilliseconds(); // temp, remove

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

        _outboundDataTimer = new System.Timers.Timer(DataRequestInterval.TotalMilliseconds);
        _outboundDataTimer.Elapsed += OnTimerTimeElapsed;
        _outboundDataTimer.Start();

        StimulatorBattery = new Percentage(0);
        ControllerBattery = new Percentage(0);
    }

    private async void OnTimerTimeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsConnected) return;

        Debug.Print("Sent");
        await SendMsg(new(
            StimulationParameters.GetDefaultValues(),
            MeasurementType.Fatigue,
            false));
    }

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        if (!_bleDeviceHandler.IsConnected) IsCurrentlyMeasuring = false;
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SendMsg(MyodamRequestMessage message)
    {
        await _bleDeviceHandler.Send(message.FormatForSending());
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
    {
        //Debug.Print("Received");
        var reply = _messageParser.ParseReply(data);
        StimulatorBattery = reply.StimulatorBattery;
        ControllerBattery = reply.ControllerBattery;
        if (reply.MeasurementStatus == MyodamMeasurement.Idle) IsCurrentlyMeasuring = false;

        if (!IsCurrentlyMeasuring) return;

        NewValueReceived?.Invoke(
            this,
            new MeasuredValue(new Random().NextDouble() * 20, //reply.SensorValue
                TimeSpan.FromMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds() - _firstTimeStamp.Value))); // TODO change when BLE part is ready: reply.Timestamp
    }


    public async Task<double> GetSensorCalibrationValue()
    {
        var calibrator = new Calibrator();

        IsCalibrating = true;
        IsCurrentlyMeasuring = true;
        try
        {
            var value = await calibrator.GetCalibrationValue(this);
            IsCurrentlyMeasuring = false;
            IsCalibrating = false;
            return value;
        }
        finally
        {
            IsCurrentlyMeasuring = false;
            IsCalibrating = false;
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurement(StimulationParameters parameters, MeasurementType measurementType)
    {
        if (IsCurrentlyMeasuring) throw new MeasurementIsAlreadyActiveException();

        CurrentParameters = parameters;
        await SendMsg(new MyodamRequestMessage(
            CurrentParameters,
            measurementType,
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

    public async Task Disconnect()
    {
        try
        {
            await StopMeasurement();
        }
        catch (System.Exception ex)
        {
            Debug.Print("Exception while disconnecting from device: " + ex.Message);
        }
        _bleDeviceHandler.Disconnect();
        _bleDeviceHandler.DataReceived -= BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged -= BleDeviceStatusChanged;
    }
}