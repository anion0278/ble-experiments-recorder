using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using BleRecorder.Business.Exception;
using BleRecorder.Common.Services;
using BleRecorder.Models;
using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;

namespace BleRecorder.Business.Device;

public class BleRecorderDevice : IBleRecorderDevice
{
    private readonly IBluetoothDeviceHandler _bleDeviceHandler;
    private readonly IBleRecorderReplyParser _messageParser;
    private readonly ISynchronizationContextProvider _synchronizationContextProvider;
    private bool _isCurrentlyMeasuring;
    private Percentage _stimulatorBattery;
    private Percentage _controllerBattery;
    private StimulationParameters? _currentParameters;
    private bool _isCalibrating;
    private BleRecorderError _error = BleRecorderError.NoError;

    public event EventHandler<MeasuredValue>? NewValueReceived;
    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? ErrorChanged;
    public event EventHandler? MeasurementStatusChanged;
    public event EventHandler? BatteryStatusChanged;

    public BleRecorderError Error
    {
        get => _error;
        private set
        {
            if (value == _error) return;
            _error = value;
            ErrorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public BleRecorderDevice(
        IBluetoothDeviceHandler bleDeviceHandler,
        IBleRecorderReplyParser messageParser,
        ISynchronizationContextProvider synchronizationContextProvider,
        DeviceCalibration deviceCalibration)
    {
        Calibration = deviceCalibration;
        _bleDeviceHandler = bleDeviceHandler;
        _messageParser = messageParser;
        _synchronizationContextProvider = synchronizationContextProvider;
        _bleDeviceHandler.DataReceived += BleDeviceHandlerDataReceived;
        _bleDeviceHandler.DeviceStatusChanged += BleDeviceStatusChanged;

        StimulatorBattery = new Percentage(0);
        ControllerBattery = new Percentage(0);
    }

    private void BleDeviceStatusChanged(object? sender, EventArgs e)
    {
        if (!_bleDeviceHandler.IsConnected)
        {
            IsCurrentlyMeasuring = false;
            IsCalibrating = false;
        }
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SendMsgAsync(BleRecorderRequestMessage message)
    {
        //Debug.Print("Msg: " + message.FormatForSending());
        await _bleDeviceHandler.SendAsync(message.FormatForSending());
    }

    private void BleDeviceHandlerDataReceived(object? sender, string data)
    {
        try
        {
            var reply = _messageParser.ParseReply(data);
            StimulatorBattery = reply.StimulatorBattery;
            ControllerBattery = reply.ControllerBattery;
            Error = reply.Error;
            IsCurrentlyMeasuring = reply.CommandStatus != BleRecorderCommand.Idle;

            if (!IsCurrentlyMeasuring && !IsCalibrating) return;

            if (reply.CommandStatus == BleRecorderCommand.IntermittentIdle) return;

            NewValueReceived?.Invoke(
                this,
                new MeasuredValue(reply.SensorValue, reply.CurrentMilliAmp, reply.Timestamp));
        }
        catch (DeviceInvalidMessageException ex)
        {
            _synchronizationContextProvider.RunInContext(() => throw ex); // 'throw;' cannot be used here
        }
        catch (System.Exception ex)
        {
            _synchronizationContextProvider.RunInContext(
                () => throw new DeviceInvalidMessageException("Error during processing data from device.", ex));
        }
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
        if (Error != BleRecorderError.NoError) throw new DeviceHasErrorException();

        if (!Calibration.IsValid()) throw new DeviceMissingCalibrationException();

        if (IsCurrentlyMeasuring) throw new MeasurementIsAlreadyActiveException();

        CurrentParameters = parameters;
        var msg = new BleRecorderRequestMessage(
            CurrentParameters,
            measurementType,
            true);
        await SendMsgAsync(msg);
    }

    public async Task StopMeasurementAsync()
    {
        var msg = new BleRecorderRequestMessage(
            CurrentParameters,
            MeasurementType.MaximumContraction,
            false);
        await SendMsgAsync(msg);
    }

    public async Task DisableFesAndDisconnectAsync()
    {
        var msg = BleRecorderRequestMessage.GetDisableFesMessage();
        await SendMsgAsync(msg);
        await DisconnectAsync();
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