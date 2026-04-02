using CarRent.Console.Models.Entities;
using CarRent.Console.Models.Enums;

namespace CarRent.Console.Services;

public static class DemoQueries
{
    public static IEnumerable<Reservation> GetTodaysStarts(IEnumerable<Reservation> reservations, DateTime date) =>
        reservations.Where(r => r.StartDate.Date == date.Date);

    public static IEnumerable<Reservation> GetTodaysEnds(IEnumerable<Reservation> reservations, DateTime date) =>
        reservations.Where(r => r.EndDate.Date == date.Date);

    public static IEnumerable<Vehicle> GetAvailableVehicles(
        IEnumerable<Vehicle> vehicles,
        IEnumerable<Reservation> reservations,
        DateTime from,
        DateTime to)
    {
        var reservedVehicleIds = reservations
            .Where(r => r.Status is ReservationStatus.Confirmed or ReservationStatus.Active)
            .Where(r => !(r.EndDate.Date < from.Date || r.StartDate.Date > to.Date))
            .Select(r => r.VehicleId)
            .Distinct()
            .ToHashSet();

        return vehicles.Where(v => v.IsActive && !reservedVehicleIds.Contains(v.Id));
    }

    public static IEnumerable<IGrouping<VehicleType, Vehicle>> VehiclesByType(IEnumerable<Vehicle> vehicles) =>
        vehicles.GroupBy(v => v.Type);

    public static Reservation? GetFirstUpcomingReservation(IEnumerable<Reservation> reservations, DateTime fromDate) =>
        reservations
            .Where(r => r.StartDate >= fromDate && r.Status == ReservationStatus.Confirmed)
            .OrderBy(r => r.StartDate)
            .FirstOrDefault();

    public static IEnumerable<ServiceRecord> ServicesDueSoon(IEnumerable<ServiceRecord> services, DateTime until) =>
        services.Where(s => s.NextRecommendedServiceDate.HasValue && s.NextRecommendedServiceDate.Value.Date <= until.Date);
}
