using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Mebster.Myodam.UI.WPF.Navigation;

namespace Mebster.Myodam.UI.WPF.TestSubjects.Converters;

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