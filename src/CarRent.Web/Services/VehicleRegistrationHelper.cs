using CarRent.Model.Entities;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Services;

public static class VehicleRegistrationHelper
{
    public static DateOnly OnYear(DateOnly regDate, int year)
    {
        var day = Math.Min(regDate.Day, DateTime.DaysInMonth(year, regDate.Month));
        return new DateOnly(year, regDate.Month, day);
    }

    public static IReadOnlyList<VehicleReservationEventVm> BuildTabEvents(Vehicle vehicle)
    {
        var events = vehicle.Reservations
            .Select(r => new VehicleReservationEventVm
            {
                SortDate = r.StartDate,
                IsRegistration = false,
                Reservation = r
            })
            .ToList();

        if (vehicle.RegistrationDueDate is not { } regDate)
            return events.OrderByDescending(e => e.SortDate).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var yearFrom = today.Year - 1;
        var yearTo = today.Year + 2;

        if (vehicle.Reservations.Count > 0)
        {
            yearFrom = Math.Min(yearFrom, vehicle.Reservations.Min(r => r.StartDate.Year) - 1);
            yearTo = Math.Max(yearTo, vehicle.Reservations.Max(r => r.EndDate.Year) + 1);
        }

        for (var year = yearFrom; year <= yearTo; year++)
        {
            var date = OnYear(regDate, year);
            events.Add(new VehicleReservationEventVm
            {
                SortDate = date.ToDateTime(TimeOnly.MinValue),
                IsRegistration = true,
                RegistrationIsPast = date < today,
                RegistrationIsToday = date == today
            });
        }

        return events.OrderByDescending(e => e.SortDate).ToList();
    }
}
