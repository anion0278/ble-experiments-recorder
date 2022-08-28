using System.Windows;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

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