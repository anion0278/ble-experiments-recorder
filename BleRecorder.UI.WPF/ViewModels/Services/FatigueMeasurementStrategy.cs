using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.ViewModels.Services;

public class IntermittentMeasurementStrategy : IMeasurementStrategy
{
    private readonly IntermittentMeasurement _measurement;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly ChartValues<MeasuredValue> _chartValues;
    private readonly IDateTimeService _dateTimeService;

    public int RemainingRepetitions { get; private set; }

    public MeasurementBase Measurement => _measurement;

    public bool IsCurrentlyMeasuring => _bleRecorderManager.IsCurrentlyMeasuring || RemainingRepetitions > 0;
    public bool CanStopMeasurement => IsCurrentlyMeasuring && !_bleRecorderManager.BleRecorderDevice!.IsCalibrating;

    public bool CanStartMeasurement => _bleRecorderManager.BleRecorderAvailability == BleRecorderAvailabilityStatus.Connected &&
                                       !IsCurrentlyMeasuring;

    public IntermittentMeasurementStrategy(
        IntermittentMeasurement measurement,
        IBleRecorderManager bleRecorderManager,
        ChartValues<MeasuredValue> chartValues,
        IDateTimeService dateTimeService)
    {
        _measurement = measurement;
        _bleRecorderManager = bleRecorderManager;
        _chartValues = chartValues;
        _dateTimeService = dateTimeService;

        _chartValues.Clear();
        _chartValues.AddRange(_measurement.MultiCycleRecord.Data
            .Select((r, index) => new MeasuredValue(
                r.MaxContraction,
                index, 
                TimeSpan.FromSeconds(index))));

        _bleRecorderManager.MeasurementStatusChanged += OnMeasurementStatusChanged;
        _bleRecorderManager.BleRecorderAvailabilityChanged += BleRecorderAvailabilityChanged;
    }

    private void BleRecorderAvailabilityChanged(object? sender, EventArgs e)
    {
        if (_bleRecorderManager.BleRecorderDevice == null || !_bleRecorderManager.BleRecorderDevice.IsConnected)
        {
            RemainingRepetitions = 0;
        }
    }

    public async Task StartMeasurement()
    {
        _bleRecorderManager.Calibration.UpdateCalibration();

        _bleRecorderManager.BleRecorderDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
        _bleRecorderManager.BleRecorderDevice!.NewValueReceived += OnNewValueReceived;

        RemainingRepetitions = _measurement.ParametersDuringMeasurement.IntermittentRepetitions;

        ClearRecordedData();
        _cancellationTokenSource = new CancellationTokenSource();
        _measurement.Date = _dateTimeService.Now;

        await StartCycle();
    }

    private async Task StartCycle()
    {
        _measurement.MultiCycleRecord.Data.Add(new SingleContractionRecord());
        await _bleRecorderManager.BleRecorderDevice.StartMeasurementAsync(_measurement.ParametersDuringMeasurement!, _measurement.Type);
    }

    private void OnNewValueReceived(object? _, MeasuredValue sensorMeasuredValue)
    {
        var forceValue = sensorMeasuredValue with
        {
            ContractionValue = _bleRecorderManager.Calibration.CalculateLoadValue(sensorMeasuredValue.ContractionValue)
        };
        _measurement.MultiCycleRecord.Data.Last().Data.Add(forceValue);
    }

    public void ClearRecordedData()
    {
        _measurement.MultiCycleRecord.Data = new List<SingleContractionRecord>(_measurement.ParametersDuringMeasurement.IntermittentRepetitions);
        _chartValues.Clear();
    }

    private async void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        // TODO solve case when device is null (was already disconnected and may cause memory leak)
        if (!_bleRecorderManager.IsCurrentlyMeasuring && _bleRecorderManager.BleRecorderDevice != null) //_bleRecorderManager.BleRecorderDevice != null
        {
            _bleRecorderManager.BleRecorderDevice.NewValueReceived -= OnNewValueReceived;
        }

        // Start next repetition
        if (_bleRecorderManager.IsCurrentlyMeasuring || RemainingRepetitions <= 0) return;

        if (_measurement.MultiCycleRecord.Data.Any() && _measurement.MultiCycleRecord.Data.First().Data.Any())
        {
            var repetition = _measurement.ParametersDuringMeasurement.IntermittentRepetitions - RemainingRepetitions;
            _chartValues.Add(new MeasuredValue(
                _measurement.MultiCycleRecord.Data.First().MaxContraction,
                repetition,
                TimeSpan.FromSeconds(repetition)));
            RemainingRepetitions--;
        }

        try
        {
            await Task.Delay(_measurement.ParametersDuringMeasurement.RestTime, _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException _)
        {
            Debug.Print("Intermittent measurement was cancelled during the resting phase.");
        }

        if (RemainingRepetitions <= 0) return;
        await StartCycle();
    }

    public async Task<bool> StopMeasurement()
    {
        RemainingRepetitions = 0;
        _cancellationTokenSource?.Cancel();
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