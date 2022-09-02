using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper.Configuration;
using DocumentFormat.OpenXml.Drawing;
using LiveCharts.Wpf;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for testSubjectDetailView.xaml
    /// </summary>
    public partial class TestSubjectDetailView : UserControl
    {
        public TestSubjectDetailView()
        {
            InitializeComponent();

            DefaultLegend customLegend = new DefaultLegend();
            customLegend.Foreground = System.Windows.Media.Brushes.White;
            StatisticsGraph.ChartLegend = customLegend;

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

