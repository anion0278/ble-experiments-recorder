using LiveCharts.Configurations;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class ChartPointMapper : CartesianMapper<MeasuredValue>
{
    public ChartPointMapper()
    {
        X(mv => mv.Timestamp.TotalSeconds);
        Y(mv => mv.Value);
    }
}