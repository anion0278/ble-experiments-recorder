using System.Windows;

namespace BleRecorder.UI.WPF.Calibration.AttachProperties;

public class AttachedRotation
{
    public static readonly DependencyProperty RotatedProperty =
        DependencyProperty.RegisterAttached("Rotated", typeof(bool), typeof(AttachedRotation), new PropertyMetadata(default(bool)));

    public static void SetRotated(UIElement element, bool value)
    {
        element.SetValue(RotatedProperty, value);
    }

    public static bool GetRotated(UIElement element)
    {
        return (bool)element.GetValue(RotatedProperty);
    }
}

public class AttachedWidth
{
    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.RegisterAttached("Width", typeof(int), typeof(AttachedWidth), new PropertyMetadata(100));

    public static void SetWidth(UIElement element, int value)
    {
        element.SetValue(WidthProperty, value);
    }

    public static int GetWidth(UIElement element)
    {
        return (int)element.GetValue(WidthProperty);
    }
}