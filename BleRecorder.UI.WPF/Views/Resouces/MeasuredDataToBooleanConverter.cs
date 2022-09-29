using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Castle.Core.Internal;
using LiveCharts;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Views.Resouces;

[ValueConversion(typeof(IEnumerable), typeof(bool))]
public class MeasuredDataToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((IEnumerable)value).IsNullOrEmpty();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}