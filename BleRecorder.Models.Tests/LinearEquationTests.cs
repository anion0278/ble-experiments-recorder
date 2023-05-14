using FluentAssertions;
using BleRecorder.Models.Device;

namespace BleRecorder.Models.Tests
{
    public class LinearEquationTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(10, 11)]
        [InlineData(100, 20)]
        [InlineData(200, 30)]
        public void CalculateLoadValue(double x, double expectedValue)
        {
            var equation = new LinearEquation(
                new CalibrationData(0, 10),
                new CalibrationData(100, 20));

            var y = equation.CalculateYValue(x);

            y.Should().Be(expectedValue);
        }
    }
}