using System;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public static class DateFormatterHelper
{
    public static long OneDayTicks { get; } = TimeSpan.FromDays(1).Ticks;

    public static long GetTotalDays(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.Ticks / OneDayTicks;
    }
}