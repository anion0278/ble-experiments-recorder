namespace Mebster.Myodam.Common.Extensions;

public static class StringExtensions
{
    public static bool ContainsCaseInsensitive(this string str, string soughtString)
    {
        return str.Contains(soughtString, StringComparison.OrdinalIgnoreCase);
    }
}