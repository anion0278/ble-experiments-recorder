using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace BleRecorder.UI.WPF.Views.Resouces;

public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
        base.OnDetaching();
    }

    void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // stop propagation to cancel the standard scrolling in datagrid
        e.Handled = true;

        var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
        e2.RoutedEvent = UIElement.MouseWheelEvent;

        // raise event to notify parent scrollview about scrolling
        AssociatedObject.RaiseEvent(e2);
    }
}