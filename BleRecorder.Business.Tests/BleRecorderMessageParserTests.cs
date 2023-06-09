﻿using FluentAssertions;
using BleRecorder.Business.Device;
using BleRecorder.Models;
using Xunit;
using Xunit.Categories;

namespace BleRecorder.Business.Tests;

public class BleRecorderMessageParserTests
{
    [Fact]
    [UnitTest]
    public void Parses_valid_message()
    {
        string inputMessage = ">TS:01234_ST:0512_AC:00098_CB:060_FB:100_EC:0_MS:1\\n";
        var parser = new BleRecorderReplyParser();
        var expectedResult = new BleRecorderReplyMessage(
            TimeSpan.FromMilliseconds(1234),
            512.0,
            0.98,
            new Percentage(60),
            new Percentage(100),
            BleRecorderError.NoError,
            BleRecorderCommand.MaximumContraction);
        AssertionOptions.AssertEquivalencyUsing((c) => c.RespectingDeclaredTypes());

        var result = parser.ParseReply(inputMessage);

        result.Should().BeEquivalentTo(expectedResult, c => c.RespectingDeclaredTypes());
    }
}