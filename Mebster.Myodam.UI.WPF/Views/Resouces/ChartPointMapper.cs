using LiveCharts.Configurations;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public class ChartPointMapper : CartesianMapper<MeasuredValue>
{
    public ChartPointMapper()
    {
        X(mv => mv.Timestamp.TotalSeconds);
        Y(mv => mv.Value);
    }
}