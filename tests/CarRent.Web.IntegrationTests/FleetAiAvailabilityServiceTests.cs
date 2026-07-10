using CarRent.DAL;
using CarRent.Web.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class FleetAiAvailabilityServiceTests
{
    [Fact]
    public async Task GetAvailabilityAsync_translates_and_returns_free_vehicles()
    {
        await using var db = CreateDb();
        var service = new FleetAiAvailabilityService(db);
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var result = await service.GetAvailabilityAsync(today, today);

        result.TotalActive.Should().BeGreaterThan(0);
        result.FreeVehicles.Count.Should().BeLessThanOrEqualTo(result.TotalActive);
    }

    private static CarRentDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase($"FleetAiAvail_{Guid.NewGuid():N}")
            .Options;
        var db = new CarRentDbContext(opts);
        db.Database.EnsureCreated();
        return db;
    }
}
