using Azure.Communication.CallAutomation;
using FluentAssertions;
using LawEnforcementDialer.Api;

namespace LawEnforcementDialer.Tests;

public class ConverterTests
{
    [Fact]
    public void ToneConverter_ConvertsToString_GivenReadOnlyList()
    {
        // arrange
        IReadOnlyList<DtmfTone> tones = new List<DtmfTone>()
        {
            new ("Zero"),
            new ("One"),
            new ("Two"),
            new ("Three")
        };

        // act
        var result = ToneConverter.ToString(tones);

        // assert
        result.Should().Be("0123");
    }
}