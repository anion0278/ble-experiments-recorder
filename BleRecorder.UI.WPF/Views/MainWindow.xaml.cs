﻿using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : MetroWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            _viewModel.MainViewClosingCommand.Execute(e);
            // cannot be handled with InvokeCommandAction
        }
    }
}
