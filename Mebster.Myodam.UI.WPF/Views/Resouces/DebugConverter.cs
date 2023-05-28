using System.Globalization;
using System;
using System.Windows.Data;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public class DebugConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}