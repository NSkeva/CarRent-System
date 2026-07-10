using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class ReservationAvailabilityHelperTests
{
    [Fact]
    public void Finds_overlap_with_confirmed_reservation()
    {
        var existing = new Reservation
        {
            Id = 10,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 10),
            EndDate = new DateTime(2026, 6, 15),
            Status = ReservationStatus.Confirmed
        };

        var candidate = new Reservation
        {
            Id = 0,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 14),
            EndDate = new DateTime(2026, 6, 18)
        };

        ReservationAvailabilityHelper.FindConflict(candidate, [existing])
            .Should().NotBeNull();
    }

    [Fact]
    public void Ignores_cancelled_reservations()
    {
        var existing = new Reservation
        {
            Id = 10,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 10),
            EndDate = new DateTime(2026, 6, 15),
            Status = ReservationStatus.Cancelled
        };

        var candidate = new Reservation
        {
            Id = 0,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 12),
            EndDate = new DateTime(2026, 6, 14)
        };

        ReservationAvailabilityHelper.FindConflict(candidate, [existing])
            .Should().BeNull();
    }

    [Fact]
    public void Allows_same_day_turnaround_when_one_ends_and_other_starts()
    {
        var returning = new Reservation
        {
            Id = 10,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 10),
            EndDate = new DateTime(2026, 6, 15),
            Status = ReservationStatus.Confirmed
        };

        var departing = new Reservation
        {
            Id = 0,
            VehicleId = 1,
            StartDate = new DateTime(2026, 6, 15),
            EndDate = new DateTime(2026, 6, 20)
        };

        ReservationAvailabilityHelper.FindConflict(departing, [returning])
            .Should().BeNull();
    }
}
