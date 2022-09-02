using System.Windows.Controls;

namespace BleRecorder.UI.WPF.Views
{
    public partial class MeasurementDetailView : UserControl
    {
        public MeasurementDetailView()
        {
            InitializeComponent();
            GraphXAxis.LabelFormatter = FormatNumericLabel;
            GraphYAxis.LabelFormatter = FormatNumericLabel;
        }

        private static string FormatNumericLabel(double value)
        {
            return value.ToString("0.0");
        }

    }
}
