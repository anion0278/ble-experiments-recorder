using System.Windows;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class AttachedText
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached("Text", typeof(string), typeof(AttachedText), new PropertyMetadata(default(string)));

    public static void SetText(UIElement element, string value)
    {
        element.SetValue(TextProperty, value);
    }

    public static string GetText(UIElement element)
    {
        return (string)element.GetValue(TextProperty);
    }
}