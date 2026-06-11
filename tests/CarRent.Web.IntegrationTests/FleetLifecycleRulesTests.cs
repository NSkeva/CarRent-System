using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetLifecycleRulesTests
{
    private static readonly DateTime Today = new(2026, 6, 11);

    [Theory]
    [InlineData(ReservationStatus.Confirmed, "2026-06-01", "2026-06-10", ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Active, "2026-06-01", "2026-06-10", ReservationStatus.Completed)]
    [InlineData(ReservationStatus.Confirmed, "2026-06-10", "2026-06-20", ReservationStatus.Active)]
    [InlineData(ReservationStatus.Active, "2026-06-10", "2026-06-20", null)]
    [InlineData(ReservationStatus.Confirmed, "2026-06-20", "2026-06-25", null)]
    [InlineData(ReservationStatus.Active, "2026-06-20", "2026-06-25", ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Cancelled, "2026-06-01", "2026-06-10", null)]
    [InlineData(ReservationStatus.Completed, "2026-06-01", "2026-06-10", null)]
    public void Reservation_status_follows_dates(
        ReservationStatus current,
        string start,
        string end,
        ReservationStatus? expected)
    {
        var result = FleetLifecycleRules.ResolveReservationStatus(
            current,
            DateTime.Parse(start),
            DateTime.Parse(end),
            Today);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ServiceStatus.Planned, "2026-06-11", ServiceStatus.InProgress)]
    [InlineData(ServiceStatus.Planned, "2026-06-10", ServiceStatus.InProgress)]
    [InlineData(ServiceStatus.Planned, "2026-06-20", null)]
    [InlineData(ServiceStatus.InProgress, "2026-06-10", ServiceStatus.Completed)]
    [InlineData(ServiceStatus.InProgress, "2026-06-11", null)]
    [InlineData(ServiceStatus.Cancelled, "2026-06-01", null)]
    [InlineData(ServiceStatus.Completed, "2026-06-01", null)]
    public void Service_status_follows_service_date(
        ServiceStatus current,
        string serviceDate,
        ServiceStatus? expected)
    {
        var result = FleetLifecycleRules.ResolveServiceStatus(
            current,
            DateTime.Parse(serviceDate),
            Today);

        result.Should().Be(expected);
    }

    [Fact]
    public void Confirmed_reservation_becomes_no_show_after_24_hours_within_period()
    {
        var start = new DateTime(2026, 6, 9, 10, 0, 0);
        var end = new DateTime(2026, 6, 20);
        var now = start.AddHours(25);

        FleetLifecycleRules.ResolveNoShow(ReservationStatus.Confirmed, start, end, now, 24)
            .Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Active_past_end_becomes_completed()
    {
        var reservation = new Reservation
        {
            Status = ReservationStatus.Active,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10)
        };

        var change = FleetLifecycleRules.EvaluateReservation(
            reservation,
            new FleetLifecycleOptions(),
            new DateTime(2026, 6, 11, 12, 0, 0));

        change.NewStatus.Should().Be(ReservationStatus.Completed);
        change.SuggestMileageUpdate.Should().BeTrue();
    }

    [Fact]
    public void Confirmed_past_end_without_pickup_becomes_cancelled()
    {
        var reservation = new Reservation
        {
            Status = ReservationStatus.Confirmed,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10)
        };

        var change = FleetLifecycleRules.EvaluateReservation(
            reservation,
            new FleetLifecycleOptions(),
            new DateTime(2026, 6, 11, 12, 0, 0));

        change.NewStatus.Should().Be(ReservationStatus.Cancelled);
        change.IsNoShowCancellation.Should().BeTrue();
        change.SuggestMileageUpdate.Should().BeFalse();
    }

    [Fact]
    public void Cancelled_reservation_is_never_changed()
    {
        var reservation = new Reservation
        {
            Status = ReservationStatus.Cancelled,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10)
        };

        var change = FleetLifecycleRules.EvaluateReservation(
            reservation,
            new FleetLifecycleOptions(),
            Today);

        change.HasStatusChange.Should().BeFalse();
    }

    [Fact]
    public void Draft_older_than_expiry_is_cancelled()
    {
        var reservation = new Reservation
        {
            Status = ReservationStatus.Draft,
            CreatedAt = Today.AddDays(-8),
            StartDate = Today.AddDays(1),
            EndDate = Today.AddDays(3)
        };

        var change = FleetLifecycleRules.EvaluateReservation(
            reservation,
            new FleetLifecycleOptions { DraftExpiryDays = 7 },
            Today);

        change.NewStatus.Should().Be(ReservationStatus.Cancelled);
        change.IsDraftExpiry.Should().BeTrue();
    }
}
