using System.Windows;
using System.Windows.Controls;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class ScrollViewerWithStableOffset : ScrollViewer
{
    private double _offset = 0;

    public ScrollViewerWithStableOffset()
    {
        Unloaded += ScrollViewerEx_Unloaded;
        Loaded += ScrollViewerEx_Loaded;
    }

    private void ScrollViewerEx_Loaded(object sender, RoutedEventArgs e)
    {
        ScrollToVerticalOffset(_offset);
    }

    private void ScrollViewerEx_Unloaded(object sender, RoutedEventArgs e)
    {
        _offset = VerticalOffset;
    }
}