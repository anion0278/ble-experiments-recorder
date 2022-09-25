using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using LiveCharts;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public class MultiCollectionToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var collections = values as IList<object>;
        if (collections.First() == DependencyProperty.UnsetValue) return Visibility.Collapsed;
        return collections.Cast<ICollection>()
            .Select(collection => collection.Count)
            .Any(c => c >= 2) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MultiCollectionToRangeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var defaultValue = DateTimeOffset.Now;

        long rangeLimit = 0;
        var collections = values as IList<object>;
        if (collections.First() == DependencyProperty.UnsetValue) return (double)defaultValue.GetTotalDays();
        var vals = collections.Cast<ChartValues<StatisticsValue>>()
            .SelectMany(c => c).Select(c => c.MeasurementDate).ToArray();
        
        if (parameter.Equals("Min")) rangeLimit = vals.DefaultIfEmpty(defaultValue).Min().GetTotalDays();
        if (parameter.Equals("Max")) rangeLimit = vals.DefaultIfEmpty(defaultValue.AddDays(1)).Max().GetTotalDays();

        return (double)rangeLimit;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
