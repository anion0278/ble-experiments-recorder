﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace BleRecorder.UI.WPF.Views.Resouces;

/// <summary>Represents a collection of <see cref="IValueConverter"/>s.</summary>
public sealed class ValueConverterCollection : Collection<IValueConverter> { }


/// <summary>Represents a chain of <see cref="IValueConverter"/>s to be executed in succession.</summary>
[ContentProperty("Converters")]
[ContentWrapper(typeof(ValueConverterCollection))]
public class ConverterPipeline : IValueConverter
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


