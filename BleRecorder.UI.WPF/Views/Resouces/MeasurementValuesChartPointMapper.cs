using System.Windows.Media;
using LiveCharts.Configurations;
using BleRecorder.Models.TestSubject;
using TimeSpan = System.TimeSpan;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class MeasurementValuesChartPointMapper : CartesianMapper<MeasuredValue>
{
    public MeasurementValuesChartPointMapper()
    {
        X(mv => mv.Timestamp.TotalSeconds);
        Y(mv => mv.Value);
    }
}

public class StatisticChartPointMapper : CartesianMapper<StatisticsValue>
{
    public StatisticChartPointMapper()
    {
        X(mv => mv.MeasurementDate.Ticks / TimeSpan.FromDays(1).Ticks);
        Y(mv => mv.ContractionForceValue);
    }
}