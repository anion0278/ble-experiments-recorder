﻿using System.Collections.Immutable;
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
    private readonly IBluetoothDeviceHandler _bleDeviceHandler;
    private readonly IMyodamMessageParser _messageParser;
    private bool _isCurrentlyMeasuring;
    private Percentage _stimulatorBattery;
    private Percentage _controllerBattery;
    private StimulationParameters? _currentParameters;
    private bool _isCalibrating;

    //private System.Timers.Timer _outboundDataTimer;
    //private MyodamMeasurement _currentRequestedMeasurementStatus = MyodamMeasurement.Idle;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;

    public DeviceCalibration Calibration { get; set; }

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

    public bool IsCalibrating // state machine would be better
    {
        get => _isCalibrating;
        private set
        {
            if (_isCalibrating == value) return;
            _isCalibrating = value;
            MeasurementStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public MyodamDevice(MyodamManager myodamManager, IBluetoothDeviceHandler bleDeviceHandler,
        IMyodamMessageParser messageParser, DeviceCalibration deviceCalibration)
    {
        Calibration = deviceCalibration;
        _myodamManager = myodamManager;
        _bleDeviceHandler = bleDeviceHandler;
        _messageParser = messageParser;
        _bleDeviceHandler.DataReceived += BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged += BleDeviceStatusChanged;

        //_outboundDataTimer = new System.Timers.Timer(DataRequestInterval.TotalMilliseconds);
        //_outboundDataTimer.Elapsed += OnTimerTimeElapsed;
        //_outboundDataTimer.Start();

        StimulatorBattery = new Percentage(0);
        ControllerBattery = new Percentage(0);
    }

    //private async void OnTimerTimeElapsed(object? sender, ElapsedEventArgs e)
    //{
    //    if (!IsConnected) return;

    //    //Debug.Print("Sent");
    //    var (x,y) = _currentRequestedMeasurementStatus switch
    //    {
    //        MyodamMeasurement.Idle => (MeasurementType.Fatigue, false),
    //        MyodamMeasurement.MaximumContraction => (MeasurementType.MaximumContraction, true),
    //        MyodamMeasurement.Fatigue => (MeasurementType.Fatigue, true),
    //        _ => throw new ArgumentOutOfRangeException()
    //    };
    
    //    var msg = new MyodamRequestMessage(CurrentParameters, x, y);
    //    await SendMsg(msg);
    //}

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        if (!_bleDeviceHandler.IsConnected)
        {
            IsCurrentlyMeasuring = false;
            IsCalibrating = false;
            // no need to set _currentRequestedMeasurementStatus
        }
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SendMsgAsync(MyodamRequestMessage message)
    {
        await _bleDeviceHandler.SendAsync(message.FormatForSending());
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
    {
        //Debug.Print("Received");
        var reply = _messageParser.ParseReply(data);
        StimulatorBattery = reply.StimulatorBattery;
        ControllerBattery = reply.ControllerBattery;
        //Debug.Print(reply.MeasurementStatus.ToString());
        IsCurrentlyMeasuring = reply.MeasurementStatus != MyodamMeasurement.Idle;

        if (!IsCurrentlyMeasuring && !IsCalibrating) return;

        NewValueReceived?.Invoke(this, new MeasuredValue(reply.SensorValue, reply.Timestamp)); 
    }


    public async Task<double> GetSensorCalibrationValueAsync()
    {
        var calibrator = new Calibrator();

        IsCalibrating = true;
        try
        {
            var value = await calibrator.GetCalibrationValueAsync(this);
            return value;
        }
        finally
        {
            IsCalibrating = false;
        }
    }

    // We always send up-to-date parameters in order to make sure that stimulation is correct even if the device has restarted in meantime
    public async Task StartMeasurementAsync(StimulationParameters parameters, MeasurementType measurementType)
    {
        if (!Calibration.IsValid()) throw new DeviceMissingCalibrationException();

        if (IsCurrentlyMeasuring) throw new MeasurementIsAlreadyActiveException();

        CurrentParameters = parameters;
        var msg = new MyodamRequestMessage(
            CurrentParameters,
            measurementType,
            true);
        //_currentRequestedMeasurementStatus = msg.Measurement;
        await SendMsgAsync(msg);
    }

    public async Task StopMeasurementAsync()
    {
        var msg = new MyodamRequestMessage(
            CurrentParameters,
            MeasurementType.MaximumContraction,
            false);
        //_currentRequestedMeasurementStatus = msg.Measurement;
        await SendMsgAsync(msg);
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await StopMeasurementAsync();
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