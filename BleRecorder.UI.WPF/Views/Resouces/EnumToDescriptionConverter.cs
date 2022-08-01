using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace BleRecorder.UI.WPF.Views.Resouces;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var enumValue = value as Enum;

        return enumValue == null ? DependencyProperty.UnsetValue : GetDescription(enumValue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public static string GetDescription(Enum en)
    {
        Type type = en.GetType();
        MemberInfo[] memInfo = type.GetMember(en.ToString());
        if (memInfo != null && memInfo.Length > 0)
        {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        return en.ToString();
    }
}