namespace BleRecorder.Common;

public static class Guard
{
    public static void ValueShouldBeMoreOrEqualThan<T>(T value, T limit) where T: IComparable<T>
    {
        if (value.CompareTo(limit) < 0)
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}