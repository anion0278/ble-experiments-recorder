﻿using System;
using System.Collections;
using System.Windows.Data;
using Castle.Core.Internal;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

[ValueConversion(typeof(IEnumerable), typeof(bool))]
public class MeasuredDataToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return ((IEnumerable)value).IsNullOrEmpty();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}