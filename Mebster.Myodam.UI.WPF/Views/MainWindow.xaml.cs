using System.Windows;
using MahApps.Metro.Controls;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Views
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

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //double offset;
            //if (PageScrollViewer.Tag != null
            //    && double.TryParse(PageScrollViewer.Tag.ToString(), out offset))
            //{
            //    PageScrollViewer.ScrollToVerticalOffset(offset);
            //}
        }

        private void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            //PageScrollViewer.Tag = PageScrollViewer.VerticalOffset;
        }
    }
}
