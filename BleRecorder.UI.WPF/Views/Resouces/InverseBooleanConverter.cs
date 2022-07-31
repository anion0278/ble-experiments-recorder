using System;
using System.Windows;
using System.Windows.Data;

namespace BleRecorder.UI.WPF.Views.Resouces;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}


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