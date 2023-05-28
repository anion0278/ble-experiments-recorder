using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BleRecorder.UI.WPF.Navigation;

namespace BleRecorder.UI.WPF.Views.Resouces;

[ValueConversion(typeof(ICollectionView), typeof(uint))]
public class NavigationItemsCollectionToCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((IEnumerable)value).OfType<INavigationItemViewModel>().Count(i => i.IsSelectedForExport);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}