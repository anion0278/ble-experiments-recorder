using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Castle.Core.Internal;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Views.Resouces;

[ValueConversion(typeof(IEnumerable), typeof(bool))]
public class MeasuredDataToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool) && targetType != typeof(object))
            throw new InvalidOperationException("The target must be a boolean");

        return ((IEnumerable)value).IsNullOrEmpty();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}


public class MultiCollectionToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var collections = values as IList<object>;
        if (collections.First() == DependencyProperty.UnsetValue) return Visibility.Collapsed;
        return collections.Cast<ICollection>().Select(collection => collection.Count).All(c => c >= 1) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
