using System;
using System.Collections.Generic;
using AutoMapper;
using LiveCharts;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services;

public interface IMeasurementFactory
{
    MeasurementBase ChangeMeasurementType(MeasurementType targetType, MeasurementBase currentMeasurement, IMapper mapper);

    MeasurementBase CreateNewEmptyMeasurement(TestSubject ts);

    IMeasurementStrategy GetMeasurementStrategy(MeasurementBase measurement, ChartValues<MeasuredValue> chartValues);
}

public class MeasurementFactory : IMeasurementFactory
{
    private readonly IMyodamManager _myodamManager;
    private readonly IDateTimeService _dateTimeService;

    public MeasurementFactory(IMyodamManager myodamManager, IDateTimeService dateTimeService)
    {
        _myodamManager = myodamManager;
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
            case MeasurementType.Fatigue:
                measurement = new FatigueMeasurement();
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
                    _myodamManager, chartValues, 
                    _dateTimeService);
            case MeasurementType.Fatigue:
                return new FatigueMeasurementStrategy(
                    (measurement as FatigueMeasurement)!,
                    _myodamManager, chartValues,
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