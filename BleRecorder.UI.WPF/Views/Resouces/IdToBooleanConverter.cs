using System;
using System.Windows.Data;

namespace BleRecorder.UI.WPF.Views.Resouces;


[ValueConversion(typeof(int), typeof(bool))]
public class IdToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return (int)value < 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}