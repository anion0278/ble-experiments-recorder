using LiveCharts.Configurations;
using Mebster.Myodam.Models.Measurements;
using Mebster.Myodam.UI.WPF.Views.Resouces;
using TimeSpan = System.TimeSpan;

namespace Mebster.Myodam.UI.WPF.Measurements.Converters;

public class MeasurementValuesChartPointMapper : CartesianMapper<MeasuredValue>
{
    public MeasurementValuesChartPointMapper()
    {
        X(mv => mv.StimulationCurrent);
        Y(mv => mv.ContractionValue);
    }
}

public class MeasurementRepetitionsChartPointMapper : CartesianMapper<MeasuredValue>
{
    public MeasurementRepetitionsChartPointMapper()
    {
        Y(mv => mv.ContractionValue);
    }
}

public class StatisticChartPointMapper : CartesianMapper<StatisticsValue>
{
    public StatisticChartPointMapper()
    {
        X(mv => mv.MeasurementDate.GetTotalDays());
        Y(mv => mv.Value);
    }
}