using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services;

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
    private readonly IMyodamManager _myodamManager;
    private readonly ChartValues<MeasuredValue> _chartValues;
    private readonly IDateTimeService _dateTimeService;
    public bool IsCurrentlyMeasuring => _myodamManager.IsCurrentlyMeasuring; // override only this prop

    public bool CanStopMeasurement => IsCurrentlyMeasuring && !_myodamManager.MyodamDevice!.IsCalibrating;

    public bool CanStartMeasurement => _myodamManager.MyodamAvailability == MyodamAvailabilityStatus.Connected &&
                                       !IsCurrentlyMeasuring;

    public MeasurementBase Measurement => _measurement;

    public MaximumContractionMeasurementStrategy(
        MaximumContractionMeasurement measurement, 
        IMyodamManager myodamManager, 
        ChartValues<MeasuredValue> chartValues, 
        IDateTimeService dateTimeService)
    {
        _measurement = measurement;
        _myodamManager = myodamManager;
        _chartValues = chartValues;
        _dateTimeService = dateTimeService;

        _chartValues.Clear();
        _chartValues.AddRange(_measurement.Record.Data);

        _myodamManager.MeasurementStatusChanged -= OnMeasurementStatusChanged;
    }


    private void OnMeasurementStatusChanged(object? sender, EventArgs e)
    {
        // TODO solve case when device is null (was already disconnected and may cause memory leak)
        if (!_myodamManager.IsCurrentlyMeasuring && _myodamManager.MyodamDevice != null) //_myodamManager.MyodamDevice != null
        {
            _myodamManager.MyodamDevice.NewValueReceived -= OnNewValueReceived;
        }
    }

    public async Task StartMeasurement()
    {
        _myodamManager.Calibration.UpdateCalibration();

        _myodamManager.MyodamDevice!.NewValueReceived -= OnNewValueReceived; // making sure that it is not subscribed multiple times
        _myodamManager.MyodamDevice!.NewValueReceived += OnNewValueReceived;

        ClearRecordedData();

        _measurement.Date = _dateTimeService.Now;

        await _myodamManager.MyodamDevice.StartMeasurementAsync(_measurement.ParametersDuringMeasurement!, _measurement.Type);
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
            ContractionValue = _myodamManager.Calibration.CalculateLoadValue(sensorMeasuredValue.ContractionValue)
        };
        _measurement.Record.Data.Add(forceValue);
        _chartValues.Add(forceValue);
    }

    public async Task<bool> StopMeasurement()
    {
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