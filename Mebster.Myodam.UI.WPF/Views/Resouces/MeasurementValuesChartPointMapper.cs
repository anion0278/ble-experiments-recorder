using System.Windows.Media;
using LiveCharts.Configurations;
using Mebster.Myodam.Models.TestSubject;
using TimeSpan = System.TimeSpan;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public class MeasurementValuesChartPointMapper : CartesianMapper<MeasuredValue>
{
    public MeasurementValuesChartPointMapper()
    {
        X(mv => mv.Timestamp.TotalSeconds);
        Y(mv => mv.ContractionValue);
    }
}

public class StatisticChartPointMapper : CartesianMapper<StatisticsValue>
{
    public StatisticChartPointMapper()
    {
        X(mv => mv.MeasurementDate.GetTotalDays());
        Y(mv => mv.ContractionForceValue);
    }
}