using System.Windows.Media;
using LiveCharts.Configurations;
using BleRecorder.Models.TestSubject;
using TimeSpan = System.TimeSpan;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class MeasurementValuesChartPointMapper : CartesianMapper<MeasuredValue>
{
    public MeasurementValuesChartPointMapper()
    {
        X(mv => mv.StimulationCurrent);
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