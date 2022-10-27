using System.Windows.Controls;

namespace BleRecorder.UI.WPF.Views
{
    public partial class MeasurementDetailView : UserControl
    {
        public MeasurementDetailView()
        {
            InitializeComponent();
            GraphXAxis.LabelFormatter = FormatNumericLabel;
            GraphYAxis.LabelFormatter = FormatYLabel;

            Graph2YAxis.LabelFormatter = FormatYLabel;
        }

        private static string FormatNumericLabel(double value)
        {
            return value.ToString("0");
        }

        private static string FormatYLabel(double value)
        {
            return value.ToString("0.0");
        }
    }
}
