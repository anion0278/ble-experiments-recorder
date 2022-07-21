﻿using System.Windows.Controls;
using System.Windows.Input;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.View
{
    /// <summary>
    /// Interaction logic for testSubjectDetailView.xaml
    /// </summary>
    public partial class testSubjectDetailView : UserControl
    {
        public testSubjectDetailView()
        {
            InitializeComponent();
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