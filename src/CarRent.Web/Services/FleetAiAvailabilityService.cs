using CarRent.DAL;
using CarRent.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services;

public sealed record FleetVehicleSnapshot(
    int Id,
    string Brand,
    string Model,
    string RegistrationNumber,
    VehicleType Type,
    decimal DailyPrice);

public sealed record FleetAvailabilityResult(
    DateOnly From,
    DateOnly To,
    int TotalActive,
    IReadOnlyList<FleetVehicleSnapshot> FreeVehicles);

public sealed class FleetAiAvailabilityService(CarRentDbContext db)
{
    public async Task<FleetAvailabilityResult> GetAvailabilityAsync(
        DateOnly from,
        DateOnly to,
        VehicleType? typeFilter = null,
        CancellationToken ct = default)
    {
        if (to < from)
            to = from;

        var rangeStart = from.ToDateTime(TimeOnly.MinValue);
        var rangeEnd = to.ToDateTime(TimeOnly.MinValue);

        var vehiclesQuery = db.Vehicles.AsNoTracking().Where(v => v.IsActive);
        if (typeFilter.HasValue)
            vehiclesQuery = vehiclesQuery.Where(v => v.Type == typeFilter.Value);

        var vehicles = await vehiclesQuery
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .Select(v => new FleetVehicleSnapshot(
                v.Id,
                v.Brand,
                v.Model,
                v.RegistrationNumber,
                v.Type,
                v.DailyPrice))
            .ToListAsync(ct);

        var busyIds = await db.Reservations.AsNoTracking()
            .Where(r =>
                (r.Status == ReservationStatus.Draft ||
                 r.Status == ReservationStatus.Confirmed ||
                 r.Status == ReservationStatus.Active) &&
                r.StartDate.Date <= rangeEnd.Date &&
                r.EndDate.Date >= rangeStart.Date)
            .Select(r => r.VehicleId)
            .Distinct()
            .ToListAsync(ct);

        var busySet = busyIds.ToHashSet();
        var free = vehicles.Where(v => !busySet.Contains(v.Id)).ToList();

        return new FleetAvailabilityResult(from, to, vehicles.Count, free);
    }

    public static string FormatVehicleType(VehicleType type) => type switch
    {
        VehicleType.Car => "Automobil",
        VehicleType.Van => "Kombi",
        VehicleType.Scooter => "Skuter",
        VehicleType.Motorcycle => "Motocikl",
        VehicleType.Bicycle => "Bicikl",
        _ => type.ToString()
    };

    public static string FormatClientAvailabilityReply(FleetAvailabilityResult result, VehicleType? typeFilter)
    {
        var period = FleetAiDateParser.FormatRange(result.From, result.To);
        var typeLabel = typeFilter.HasValue ? $" ({FormatVehicleType(typeFilter.Value)})" : string.Empty;

        if (result.TotalActive == 0)
            return $"Trenutno nemamo aktivnih vozila{typeLabel} u sustavu.";

        if (result.FreeVehicles.Count == 0)
            return $"Za period {period} nema slobodnih vozila{typeLabel} ({result.TotalActive} zauzeto). " +
                   "Pokušajte druge datume ili drugi tip vozila.";

        var lines = result.FreeVehicles
            .Take(8)
            .Select(v => $"- {v.Brand} {v.Model} ({v.RegistrationNumber}), {FormatVehicleType(v.Type)}, {v.DailyPrice:N0} EUR/dan");

        var more = result.FreeVehicles.Count > 8
            ? $"\n… i još {result.FreeVehicles.Count - 8} vozila."
            : string.Empty;

        return $"Za period {period} slobodno je {result.FreeVehicles.Count} od {result.TotalActive} vozila{typeLabel}:\n" +
               string.Join("\n", lines) + more +
               "\n\nZa rezervaciju navedite točne datume i model ili se javite našem timu.";
    }
}
