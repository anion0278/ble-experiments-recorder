using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.ViewModels.Services;

public interface IMeasurementStrategy : IDisposable
{
    MeasurementBase Measurement { get; }

    bool IsCurrentlyMeasuring { get; }
    bool CanStopMeasurement { get; }
    bool CanStartMeasurement { get; }
    Task StartMeasurement();
    void ClearRecordedData();
    Task<bool> StopMeasurement();
}


public class MaximumContractionMeasurementStrategy : IMeasurementStrategy
{
    private readonly MaximumContractionMeasurement _measurement;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly ChartValues<MeasuredValue> _chartValues;
    private readonly IDateTimeService _dateTimeService;
    public bool IsCurrentlyMeasuring => _bleRecorderManager.IsCurrentlyMeasuring; // override only this prop

    public bool CanStopMeasurement => IsCurrentlyMeasuring && !_bleRecorderManager.BleRecorderDevice!.IsCalibrating;

    public bool CanStartMeasurement => _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected &&
                                       !IsCurrentlyMeasuring;

    public MeasurementBase Measurement => _measurement;

    public MaximumContractionMeasurementStrategy(
        MaximumContractionMeasurement measurement, 
        IBleRecorderManager bleRecorderManager, 
        ChartValues<MeasuredValue> chartValues, 
        IDateTimeService dateTimeService)
    {
        _measurement = measurement;
        _bleRecorderManager = bleRecorderManager;
        _chartValues = chartValues;
        _dateTimeService = dateTimeService;

        _chartValues.Clear();
        _chartValues.AddRange(_measurement.Record.Data);

        _bleRecorderManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;
    }


    private void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        // TODO solve case when device is null (was already disconnected and may cause memory leak)
        if (!_bleRecorderManager.IsCurrentlyMeasuring && _bleRecorderManager.BleRecorderDevice != null) //_bleRecorderManager.BleRecorderDevice != null
        {
            _bleRecorderManager.BleRecorderDevice.NewValueReceived -= OnNewValueReceived;
        }
    }

    public async Task StartMeasurement()
    {
        _bleRecorderManager.Calibration.UpdateCalibration();

        _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
        _bleRecorderManager.BleRecorderDevice!.NewValueReceived += OnNewValueReceived;

        ClearRecordedData();

        _measurement.Date = _dateTimeService.Now;

        await _bleRecorderManager.BleRecorderDevice.StartMeasurementAsync(_measurement.ParametersDuringMeasurement!, _measurement.Type);
    }

    public void ClearRecordedData()
    {
        _measurement.Record.Data = new List<MeasuredValue>();
        _chartValues.Clear();
    }

    private void OnNewValueReceived(object? _, MeasuredValue sensorMeasuredValue)
    {
        var forceValue = sensorMeasuredValue with
        {
            ContractionValue = _bleRecorderManager.Calibration.CalculateLoadValue(sensorMeasuredValue.ContractionValue)
        };
        _measurement.Record.Data.Add(forceValue);
        _chartValues.Add(forceValue);
    }

    public async Task<bool> StopMeasurement()
    {
        if (_bleRecorderManager.BleRecorderDevice == null) return false;

        await _bleRecorderManager.BleRecorderDevice.StopMeasurementAsync();
        _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived;
        ClearRecordedData();

        return _bleRecorderManager.IsCurrentlyMeasuring;
    }

    public void Dispose()
    {
        _bleRecorderManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;
        if (_bleRecorderManager.BleRecorderDevice != null) _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived;
    }
}