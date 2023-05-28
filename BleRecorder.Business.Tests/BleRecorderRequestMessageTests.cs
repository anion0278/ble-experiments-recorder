using FluentAssertions;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using Xunit;
using Xunit.Categories;

namespace BleRecorder.Business.Tests
{
    public class BleRecorderRequestMessageTests
    {
        [Theory]
        [UnitTest]
        [InlineData(100, 80, 4, MeasurementType.MaximumContraction, true, ">SC:100_SF:080_SP:400_ST:10_RT:03_FR:04_MC:1\n")]
        [InlineData(8, 100, 0, MeasurementType.Intermittent, false, ">SC:008_SF:100_SP:050_ST:10_RT:03_FR:04_MC:0\n")]
        [InlineData(8, 100, 0, MeasurementType.Intermittent, true, ">SC:008_SF:100_SP:050_ST:05_RT:03_FR:04_MC:2\n")]
        public void Formats_valid_message_to_string(
            int current, 
            int frequency, 
            int pulseWidthOption, 
            MeasurementType measurementType, 
            bool isMeasurementRequested, 
            string expectedResult)
        {
            var msg = new BleRecorderRequestMessage(new StimulationParameters(
                current,
                frequency,
                StimulationPulseWidth.AvailableOptions[pulseWidthOption],
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(3), 
                4),
                measurementType,
                isMeasurementRequested);

            var result = msg.FormatForSending();

            result.Should().Be(expectedResult);
        }
    }
}