using FluentAssertions;
using BleRecorder.Business.Device;
using Xunit;

namespace BleRecorder.Business.Tests;

public class BleRecorderMessageParserTests
{
    [Fact]
    public void Parses_valid_message()
    {
        string inputMessage = ">TS:01234_ST:0512_CB:060_FB:100_EC:0_MS:1\\n";
        var parser = new BleRecorderMessageParser();
        var expectedResult = new BleRecorderReplyMessage(
            TimeSpan.FromMilliseconds(1234),
            256.0,
            new Percentage(60),
            new Percentage(100),
            BleRecorderError.NoError,
            BleRecorderMeasurement.MaximumContraction);
        AssertionOptions.AssertEquivalencyUsing((c) => c.RespectingDeclaredTypes());

        var result = parser.ParseReply(inputMessage);

        result.Should().BeEquivalentTo(expectedResult, c => c.RespectingDeclaredTypes());
    }
}