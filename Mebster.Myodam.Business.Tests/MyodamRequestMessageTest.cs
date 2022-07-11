using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Xunit;

namespace Mebster.Myodam.Business.Tests
{
    public class MyodamRequestMessageTest
    {
        [Theory]
        [InlineData(100, 80, 4, MeasurementType.MaximumContraction, true, "SC:100_SF:080_SP:400_ST:10_MC:1")]
        [InlineData(8, 100, 0, MeasurementType.Fatigue, false, "SC:008_SF:100_SP:050_ST:10_MC:0")]
        public void Formats_valid_message_to_string(
            int current, 
            int frequency, 
            int pulseWidthOption, 
            MeasurementType measurementType, 
            bool isMeasurementRequested, 
            string expectedResult)
        {
            var msg = new MyodamRequestMessage(new StimulationParameters(
                current,
                frequency,
                StimulationPulseWidth.AvailableOptions[pulseWidthOption],
                TimeSpan.FromSeconds(5)),
                measurementType,
                isMeasurementRequested);

            Assert.Equal(expectedResult, msg.FormatForSending());
        }
    }
}