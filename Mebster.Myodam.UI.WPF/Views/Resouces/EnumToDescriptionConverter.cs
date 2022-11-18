using Mebster.Myodam.UI.WPF.Extensions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Mebster.Myodam.Common.Extensions;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var enumValue = value as Enum;
        return enumValue?.GetDescription() ?? DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}