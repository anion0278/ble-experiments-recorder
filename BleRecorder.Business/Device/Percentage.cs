namespace BleRecorder.Business.Device;

public readonly struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value) 
    {
        Value = value;
        if (value is > 100 or < 0) throw new ArgumentException("Percentage is out of range");
    }

    public bool Equals(Percentage other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Percentage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static explicit operator Percentage(decimal d)
    {
        return new Percentage(d);
    }

    public static Percentage Parse(string value)
    {
        return new Percentage(decimal.Parse(value));
    }

    public static bool TryParse(string value, out Percentage percentage)
    {
        bool flag = decimal.TryParse(value, out var parsedValue);
        percentage = new Percentage(parsedValue);
        return flag;
    }

    public override string ToString()
    {
        return $"{Value}%";
    }
}