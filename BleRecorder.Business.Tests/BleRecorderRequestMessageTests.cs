using FluentAssertions;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using Xunit;

namespace BleRecorder.Business.Tests
{
    public class BleRecorderRequestMessageTests
    {
        [Theory]
        [InlineData(100, 80, 4, MeasurementType.MaximumContraction, true, ">SC:100_SF:080_SP:400_ST:10_MC:1")]
        [InlineData(8, 100, 0, MeasurementType.Intermittent, false, ">SC:008_SF:100_SP:050_ST:10_MC:0")]
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
                TimeSpan.FromSeconds(10)),
                measurementType,
                isMeasurementRequested);

            var result = msg.FormatForSending();

            result.Should().Be(expectedResult);
        }
    }
}