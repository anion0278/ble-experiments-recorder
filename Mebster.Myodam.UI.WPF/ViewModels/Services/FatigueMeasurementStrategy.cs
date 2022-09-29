using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services;

public class FatigueMeasurementStrategy : IMeasurementStrategy
{
    private readonly FatigueMeasurement _measurement;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IMyodamManager _myodamManager;
    private readonly ChartValues<MeasuredValue> _chartValues;
    private readonly IDateTimeService _dateTimeService;

    public int RemainingRepetitions { get; private set; }

    public MeasurementBase Measurement => _measurement;

    public bool IsCurrentlyMeasuring => _myodamManager.IsCurrentlyMeasuring || RemainingRepetitions > 0;
    public bool CanStopMeasurement => IsCurrentlyMeasuring && !_myodamManager.MyodamDevice!.IsCalibrating;

    public bool CanStartMeasurement => _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected &&
                                       !IsCurrentlyMeasuring;

    public FatigueMeasurementStrategy(
        FatigueMeasurement measurement,
        IMyodamManager myodamManager,
        ChartValues<MeasuredValue> chartValues,
        IDateTimeService dateTimeService)
    {
        _measurement = measurement;
        _myodamManager = myodamManager;
        _chartValues = chartValues;
        _dateTimeService = dateTimeService;

        _chartValues.Clear();
        _chartValues.AddRange(_measurement.MultiCycleRecord.Data
            .Select((r, index) => new MeasuredValue(
                r.MaxContraction,
                index, 
                TimeSpan.FromSeconds(index))));

        _myodamManager.MeasurementStatusChanged += OnMeasurementStatusChanged;
        _myodamManager.MyodamAvailabilityChanged += MyodamAvailabilityChanged;
    }

    private void MyodamAvailabilityChanged(object? sender, EventArgs e)
    {
        if (_myodamManager.MyodamDevice == null || !_myodamManager.MyodamDevice.IsConnected)
        {
            RemainingRepetitions = 0;
        }
    }

    public async Task StartMeasurement()
    {
        _myodamManager.Calibration.UpdateCalibration();

        _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
        _myodamManager.MyodamDevice!.NewValueReceived += OnNewValueReceived;

        RemainingRepetitions = _measurement.ParametersDuringMeasurement.FatigueRepetitions;

        ClearRecordedData();
        _cancellationTokenSource = new CancellationTokenSource();
        _measurement.Date = _dateTimeService.Now;

        await StartCycle();
    }

    private async Task StartCycle()
    {
        _measurement.MultiCycleRecord.Data.Add(new SingleContractionRecord());
        await _myodamManager.MyodamDevice.StartMeasurementAsync(_measurement.ParametersDuringMeasurement!, _measurement.Type);
    }

    private void OnNewValueReceived(object? _, MeasuredValue sensorMeasuredValue)
    {
        var forceValue = sensorMeasuredValue with
        {
            ContractionValue = _myodamManager.Calibration.CalculateLoadValue(sensorMeasuredValue.ContractionValue)
        };
        _measurement.MultiCycleRecord.Data.Last().Data.Add(forceValue);
    }

    public void ClearRecordedData()
    {
        _measurement.MultiCycleRecord.Data = new List<SingleContractionRecord>(_measurement.ParametersDuringMeasurement.FatigueRepetitions);
        _chartValues.Clear();
    }

    private async void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        // TODO solve case when device is null (was already disconnected and may cause memory leak)
        if (!_myodamManager.IsCurrentlyMeasuring && _myodamManager.MyodamDevice != null) //_myodamManager.MyodamDevice != null
        {
            _myodamManager.MyodamDevice.NewValueReceived -= OnNewValueReceived;
        }

        // Start next repetition
        if (_myodamManager.IsCurrentlyMeasuring || RemainingRepetitions <= 0) return;

        if (_measurement.MultiCycleRecord.Data.Any() && _measurement.MultiCycleRecord.Data.First().Data.Any())
        {
            var repetition = _measurement.ParametersDuringMeasurement.FatigueRepetitions - RemainingRepetitions;
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
            Debug.Print("Fatigue measurement was cancelled during the resting phase.");
        }

        if (RemainingRepetitions <= 0) return;
        await StartCycle();
    }

    public async Task<bool> StopMeasurement()
    {
        RemainingRepetitions = 0;
        _cancellationTokenSource?.Cancel();
        if (_myodamManager.MyodamDevice == null) return false;

        await _myodamManager.MyodamDevice.StopMeasurementAsync();
        _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived;
        ClearRecordedData();

        return _myodamManager.IsCurrentlyMeasuring;
    }

    public void Dispose()
    {
        _myodamManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;
        if (_myodamManager.MyodamDevice != null) _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived;
    }
}