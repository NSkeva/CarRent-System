using System.Net;
using System.Net.Http.Json;
using CarRent.Web.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class TimelineApiTests : IClassFixture<CarRentWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TimelineApiTests(CarRentWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task PatchSchedule_WhenUnauthorized_ReturnsUnauthorized()
    {
        var payload = new TimelineReservationSchedulePatchDto
        {
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 5),
            EndDate = new DateTime(2026, 6, 8)
        };

        var response = await _client.AsAnonymous()
            .PatchAsJsonAsync("/api/timeline/reservation/1/schedule", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchSchedule_WhenValid_UpdatesReservation()
    {
        var client = _client.AsRole("Manager");
        var list = await client.GetFromJsonAsync<List<ReservationDto>>("/api/reservation");
        list.Should().NotBeNull().And.NotBeEmpty();
        var reservation = list!.First(r => r.Status is CarRent.Model.Enums.ReservationStatus.Confirmed
            or CarRent.Model.Enums.ReservationStatus.Draft
            or CarRent.Model.Enums.ReservationStatus.Active);

        var payload = new TimelineReservationSchedulePatchDto
        {
            VehicleId = reservation.VehicleId,
            StartDate = reservation.StartDate.Date.AddDays(30),
            EndDate = reservation.StartDate.Date.AddDays(33)
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/timeline/reservation/{reservation.Id}/schedule",
            payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TimelineScheduleResultDto>();
        body!.StartDate.Date.Should().Be(payload.StartDate.Date);
        body.EndDate.Date.Should().Be(payload.EndDate.Date);
    }

    [Fact]
    public async Task PatchSchedule_WhenInvalidDates_ReturnsBadRequest()
    {
        var client = _client.AsRole("Manager");
        var list = await client.GetFromJsonAsync<List<ReservationDto>>("/api/reservation");
        var reservation = list!.First();

        var payload = new TimelineReservationSchedulePatchDto
        {
            VehicleId = reservation.VehicleId,
            StartDate = new DateTime(2026, 8, 10),
            EndDate = new DateTime(2026, 8, 5)
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/timeline/reservation/{reservation.Id}/schedule",
            payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
