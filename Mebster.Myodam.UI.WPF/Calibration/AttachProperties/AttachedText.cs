using System.Windows;

namespace Mebster.Myodam.UI.WPF.Calibration.AttachProperties;

public class AttachedText // TODO check https://stackoverflow.com/questions/66750044/can-we-implement-dependency-property-in-a-class-that-does-not-inherit-from-depen
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