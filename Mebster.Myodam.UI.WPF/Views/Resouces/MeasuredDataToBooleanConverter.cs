using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Castle.Core.Internal;
using LiveCharts;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

[ValueConversion(typeof(IEnumerable), typeof(bool))]
public class MeasuredDataToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        //if (targetType != typeof(bool) && targetType != typeof(object))
        //    throw new InvalidOperationException("The target must be a boolean");

        return ((IEnumerable)value).IsNullOrEmpty();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>Represents a chain of <see cref="IValueConverter"/>s to be executed in succession.</summary>
[ContentProperty("Converters")]
[ContentWrapper(typeof(ValueConverterCollection))]
public class ConverterChain : IValueConverter
{
    /// <summary>Gets the converters to execute.</summary>
    public ValueConverterCollection Converters { get; } = new();

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Converters
            .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Converters
            .Reverse()
            .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
    }

    #endregion
}

/// <summary>Represents a collection of <see cref="IValueConverter"/>s.</summary>
public sealed class ValueConverterCollection : Collection<IValueConverter> { }



public class MultiCollectionToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var collections = values as IList<object>;
        if (collections.First() == DependencyProperty.UnsetValue) return Visibility.Collapsed;
        return collections.Cast<ICollection>()
            .Select(collection => collection.Count)
            .All(c => c >= 1) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
