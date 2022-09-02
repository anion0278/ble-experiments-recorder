using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using LiveCharts;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Views.Resouces;

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

//public class MultiCollectionToBooleanConverter : IMultiValueConverter
//{
//    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
//    {
//        var collections = values as IList<object>;
//        if (collections.First() == DependencyProperty.UnsetValue) return false;
//        if (collections[0] is not ChartValues<StatisticsValue> maxContractionVals ||
//            collections[1] is not ChartValues<StatisticsValue> fatigueVals)
//        {
//            throw new ArgumentException("Invalid use of converter");
//        }

//        if (maxContractionVals.Count == 1 && fatigueVals.Count == 1)
//        {
//            return maxContractionVals.Single().MeasurementDate.Date.Ticks == fatigueVals.Single().MeasurementDate.Date.Ticks;
//        }
//        throw new ArgumentException("Invalid use of converter");
//    }

//    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}