using System;
using System.Collections.Generic;
using AutoMapper;
using LiveCharts;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Data.Repositories;

namespace BleRecorder.UI.WPF.ViewModels.Services;

public interface IMeasurementFactory
{
    MeasurementBase ChangeMeasurementType(MeasurementType targetType, MeasurementBase currentMeasurement, IMapper mapper);

    MeasurementBase CreateNewEmptyMeasurement(TestSubject ts);

    IMeasurementStrategy GetMeasurementStrategy(MeasurementBase measurement, ChartValues<MeasuredValue> chartValues);
}

public class MeasurementFactory : IMeasurementFactory
{
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly IDateTimeService _dateTimeService;

    public MeasurementFactory(IBleRecorderManager bleRecorderManager, IDateTimeService dateTimeService)
    {
        _bleRecorderManager = bleRecorderManager;
        _dateTimeService = dateTimeService;
    }

    public MeasurementBase ChangeMeasurementType(
        MeasurementType targetType, 
        MeasurementBase currentMeasurement, 
        IMapper mapper)
    {
        MeasurementBase measurement;
        switch (targetType)
        {
            case MeasurementType.MaximumContraction:
                measurement = new MaximumContractionMeasurement();
                mapper.Map(currentMeasurement, measurement);
                break;
            case MeasurementType.Intermittent:
                measurement = new IntermittentMeasurement();
                mapper.Map(currentMeasurement, measurement);
                break;
            default:
                throw new ArgumentException();
        }

        return measurement;
    }

    public IMeasurementStrategy GetMeasurementStrategy(MeasurementBase measurement, ChartValues<MeasuredValue> chartValues)
    {
        switch (measurement.Type)
        {
            // todo Visitor p
            case MeasurementType.MaximumContraction:
                return new MaximumContractionMeasurementStrategy(
                    (measurement as MaximumContractionMeasurement)!, 
                    _bleRecorderManager, chartValues, 
                    _dateTimeService);
            case MeasurementType.Intermittent:
                return new IntermittentMeasurementStrategy(
                    (measurement as IntermittentMeasurement)!,
                    _bleRecorderManager, chartValues,
                    _dateTimeService);
            default:
                throw new ArgumentException();
        }
    }

    public MeasurementBase CreateNewEmptyMeasurement(TestSubject ts)
    {
        return new MaximumContractionMeasurement()
        {
            TestSubject = ts
        };
    }
}