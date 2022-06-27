using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Mebster.Myodam.UI.WPF.View.Resouces;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}

public class Behaviours
{
    public static DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.RegisterAttached("DoubleClickCommand", typeof(ICommand), typeof(Behaviours),
            new PropertyMetadata(DoubleClick_PropertyChanged));

    public static void SetDoubleClickCommand(UIElement element, ICommand value)
    {
        element.SetValue(DoubleClickCommandProperty, value);
    }

    public static ICommand GetDoubleClickCommand(UIElement element)
    {
        return (ICommand)element.GetValue(DoubleClickCommandProperty);
    }

    private static void DoubleClick_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var row = d as DataGridRow;
        if (row == null) return;

        if (e.NewValue != null)
        {
            row.AddHandler(DataGridRow.MouseDoubleClickEvent, new RoutedEventHandler(DataGrid_MouseDoubleClick));
        }
        else
        {
            row.RemoveHandler(DataGridRow.MouseDoubleClickEvent, new RoutedEventHandler(DataGrid_MouseDoubleClick));
        }
    }

    private static void DataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
    {
        var row = sender as DataGridRow;

        if (row != null)
        {
            var cmd = GetDoubleClickCommand(row);
            if (cmd.CanExecute(row.Item))
                cmd.Execute(row.Item);
        }
    }
}