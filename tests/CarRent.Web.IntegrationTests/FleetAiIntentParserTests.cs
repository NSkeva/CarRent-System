using CarRent.Web.Services;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetAiIntentParserTests
{
    [Theory]
    [InlineData("trebao bi mi auto ovaj vikend", true)]
    [InlineData("Ima li slobodnih vozila?", false)]
    [InlineData("želim rezervirati kombi", true)]
    public void IsRentalNeed_detects_natural_requests(string text, bool expected)
    {
        FleetAiIntentParser.IsRentalNeed(text).Should().Be(expected);
    }

    [Theory]
    [InlineData("ok ja bi taj golf 7 dana", 7)]
    [InlineData("na tjedan dana", 7)]
    public void TryParseRentalDays_parses_duration(string text, int expected)
    {
        FleetAiIntentParser.TryParseRentalDays(text).Should().Be(expected);
    }

    [Fact]
    public void VehicleMatcher_finds_golf_from_offer_list()
    {
        var session = new FleetClientChatSession
        {
            LastOffered =
            [
                new FleetOfferedVehicle { Id = 1, Brand = "Volkswagen", Model = "Golf", RegistrationNumber = "ZG-1", DailyPrice = 50 },
                new FleetOfferedVehicle { Id = 2, Brand = "Skoda", Model = "Octavia", RegistrationNumber = "ZG-2", DailyPrice = 55 }
            ]
        };

        var match = FleetAiVehicleMatcher.Match("ok golf 7 dana", session);
        match.Should().NotBeNull();
        match!.Model.Should().Be("Golf");
    }
}
