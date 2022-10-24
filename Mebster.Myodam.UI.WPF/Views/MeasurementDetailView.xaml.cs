using System.Windows.Controls;

namespace Mebster.Myodam.UI.WPF.Views
{
    public partial class MeasurementDetailView : UserControl
    {
        public MeasurementDetailView()
        {
            InitializeComponent();
            GraphXAxis.LabelFormatter = FormatNumericLabel;
            GraphYAxis.LabelFormatter = FormatNumericLabel;

            Graph2YAxis.LabelFormatter = FormatNumericLabel;
        }

        private static string FormatNumericLabel(double value)
        {
            return value.ToString("0");
        }

    }
}
