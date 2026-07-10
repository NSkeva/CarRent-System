using CarRent.DAL;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services;

public static class VehicleDefaultImageBootstrap
{
    public static async Task ApplyAsync(CarRentDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var vehicles = await db.Vehicles.Where(v => v.Id <= 6).ToListAsync(ct);
        var updated = false;

        foreach (var vehicle in vehicles)
        {
            var path = VehicleImageHelper.GetSeedDefaultPath(vehicle.Id);
            if (path is null || vehicle.MainImagePath == path)
                continue;

            vehicle.MainImagePath = path;
            updated = true;
        }

        if (!updated)
            return;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Default slike postavljene na {Count} seed vozila.", vehicles.Count);
    }
}
