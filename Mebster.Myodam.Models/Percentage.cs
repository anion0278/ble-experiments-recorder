namespace Mebster.Myodam.Models;

public readonly struct Percentage
{
    public double Value { get; }

    public Percentage(double value, bool validate = true) 
    {
        Value = value;
        if (validate && (value is > 100 or < 0)) throw new ArgumentException("Percentage is out of range");
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

    public static implicit operator Percentage(double d)
    {
        return new Percentage(d);
    }

    public static Percentage Parse(string value)
    {
        return new Percentage(double.Parse(value));
    }

    public static bool TryParse(string value, out Percentage percentage)
    {
        bool flag = double.TryParse(value, out var parsedValue);
        percentage = new Percentage(parsedValue);
        return flag;
    }

    public string ToString(string? format)
    {
        return $"{Value.ToString(format)}%";
    }

    public override string ToString()
    {
        return ToString("0.00");
    }
}