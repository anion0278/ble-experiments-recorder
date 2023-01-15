using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper.Configuration;
using DocumentFormat.OpenXml.Drawing;
using LiveCharts.Wpf;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.Views.Resouces;

namespace BleRecorder.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for testSubjectDetailView.xaml
    /// </summary>
    public partial class TestSubjectDetailView : UserControl
    {
        private readonly long _oneDayTicks = DateFormatterHelper.OneDayTicks;

        public TestSubjectDetailView()
        {
            InitializeComponent();

            StatisticsGraphXAxis.LabelFormatter = FormatXAxisLabel;

            MaxContractionAxis.LabelFormatter = FormatNumericLabel;
            IntermittentAxis.LabelFormatter = FormatNumericLabel;
        }

        private static string FormatNumericLabel(double value)
        {
            return value.ToString("0");
        }

        private string FormatXAxisLabel(double value)
        {
            // Livechars separator does not allow for non-constant distances between ticks and default auto ticks are very off!
            // maybe another approach would be to use Livechars Sections
            if (DataContext is TestSubjectDetailViewModel vm
                && vm.MaxContractionStatisticValues.Union(vm.IntermittentStatisticValues)
                    .Any(v => v.MeasurementDate.GetTotalDays() == value))
            {
                return new DateTime((long)(value * _oneDayTicks)).ToString("dd. MMM yyyy");
            }

            return string.Empty;
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

