using CarRent.Model.Enums;
using CarRent.Web.Services;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetAiDateParserTests
{
    [Theory]
    [InlineData("15.6.-20.6.", 15, 6, 20, 6)]
    public void TryParseRange_parses_date_range(string input, int d1, int m1, int d2, int m2)
    {
        var (from, to) = FleetAiDateParser.TryParseRange(input);

        from.Should().NotBeNull();
        to.Should().NotBeNull();
        from!.Value.Day.Should().Be(d1);
        from.Value.Month.Should().Be(m1);
        to!.Value.Day.Should().Be(d2);
        to.Value.Month.Should().Be(m2);
    }

    [Fact]
    public void TryParseRange_parses_weekend()
    {
        var (from, to) = FleetAiDateParser.TryParseRange("ovaj vikend");

        from.Should().NotBeNull();
        to.Should().NotBeNull();
        from!.Value.DayOfWeek.Should().Be(DayOfWeek.Saturday);
        to!.Value.DayOfWeek.Should().Be(DayOfWeek.Sunday);
    }

    [Theory]
    [InlineData("trebam kombi", VehicleType.Van)]
    [InlineData("skuter za vikend", VehicleType.Scooter)]
    public void TryParseVehicleType_detects_type(string input, VehicleType expected)
    {
        FleetAiDateParser.TryParseVehicleType(input).Should().Be(expected);
    }
}
