namespace Mebster.Myodam.Models.Device;

public class LinearEquation
{
    private LinearEquationCoefficients _coefficients;

    public LinearEquation(CalibrationData p1, CalibrationData p2)
    {
        // TODO validation + test

        UpdateCoefficients(p1, p2);
    }

    public void UpdateCoefficients(CalibrationData p1, CalibrationData p2)
    {
        var m = (p2.Y - p1.Y) / (p2.X - p1.X);
        var b = p1.Y - (m * p1.X);

        _coefficients = new LinearEquationCoefficients(m, b);
    }

    public double CalculateYValue(double xValue)
    {
        return xValue * _coefficients.M + _coefficients.B;
    }
}


public record CalibrationData(double X, double Y);

// equation in form y = mx + b
public record LinearEquationCoefficients(double M, double B);