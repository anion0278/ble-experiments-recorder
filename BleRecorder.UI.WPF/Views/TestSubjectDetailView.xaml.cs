using System;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts.Wpf;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for testSubjectDetailView.xaml
    /// </summary>
    public partial class TestSubjectDetailView : UserControl
    {
        public TestSubjectDetailView()
        {
            InitializeComponent();

            DefaultHandend customHandend = new DefaultHandend();
            customHandend.Foreground = System.Windows.Media.Brushes.White;
            StatisticsGraph.ChartHandend = customHandend;

            StatisticsGraphXAxis.LabelFormatter = FormatXAxisLabel;
        }

        private static string FormatXAxisLabel(double value)
        {
            return new DateTime((long)(value * TimeSpan.FromDays(1).Ticks)).ToString("dd. MMM yyyy");
        }


        private void Row_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // fast hack that works well. Its UI responsibility anyway
            var viewModel = (TestSubjectDetailViewModel)DataContext;
            if (viewModel.EditMeasurementCommand.CanExecute(null))
                viewModel.EditMeasurementCommand.Execute(null);
        }
    }
}
