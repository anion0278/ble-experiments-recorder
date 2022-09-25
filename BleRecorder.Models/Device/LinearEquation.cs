namespace BleRecorder.Models.Device;

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
        _coefficients = new LinearEquationCoefficients(
            p1.Y - p2.Y,
            p2.X - p1.X,
            (p1.X - p2.X) * p1.Y + (p2.Y - p1.Y) * p1.X);
    }

    public double CalculateLoadValue(double sensorValue)
    {
        return (-_coefficients.C - _coefficients.A * sensorValue) / _coefficients.B;
    }
}


public record CalibrationData(double X, double Y);

// equation in form ax + by + c = 0
public record LinearEquationCoefficients(double A, double B, double C);