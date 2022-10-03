using FluentAssertions;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models;
using Xunit;

namespace Mebster.Myodam.Business.Tests;

public class MyodamMessageParserTests
{
    [Fact]
    public void Parses_valid_message()
    {
        string inputMessage = ">TS:01234_ST:0512_AC:098_CB:060_FB:100_EC:0_MS:1\\n";
        var parser = new MyodamReplyParser();
        var expectedResult = new MyodamReplyMessage(
            TimeSpan.FromMilliseconds(1234),
            512.0,
            98,
            new Percentage(60),
            new Percentage(100),
            MyodamError.NoError,
            MyodamMeasurement.MaximumContraction);
        AssertionOptions.AssertEquivalencyUsing((c) => c.RespectingDeclaredTypes());

        var result = parser.ParseReply(inputMessage);

        result.Should().BeEquivalentTo(expectedResult, c => c.RespectingDeclaredTypes());
    }
}