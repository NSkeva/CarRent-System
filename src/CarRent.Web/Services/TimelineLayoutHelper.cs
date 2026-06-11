using CarRent.Model.Entities;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Services;

public static class TimelineLayoutHelper
{
    public static IReadOnlyList<TimelineBarVm> BuildBars(Vehicle vehicle, IEnumerable<Reservation> reservations, DateOnly month)
    {
        var bars = BuildReservationBars(reservations, month).ToList();
        bars.AddRange(BuildRegistrationBars(vehicle, month));
        return bars.OrderBy(b => b.StartDay).ToList();
    }

    private static IEnumerable<TimelineBarVm> BuildReservationBars(IEnumerable<Reservation> reservations, DateOnly month)
    {
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var monthStart = new DateOnly(month.Year, month.Month, 1);
        var monthEnd = new DateOnly(month.Year, month.Month, daysInMonth);

        foreach (var r in reservations)
        {
            var start = DateOnly.FromDateTime(r.StartDate);
            var end = DateOnly.FromDateTime(r.EndDate);
            if (end < monthStart || start > monthEnd) continue;

            var visibleStart = start < monthStart ? monthStart : start;
            var visibleEnd = end > monthEnd ? monthEnd : end;
            var span = visibleEnd.Day - visibleStart.Day + 1;
            if (span < 1) continue;

            var label = r.Customer is null ? $"#{r.Id}" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            yield return new TimelineBarVm
            {
                Reservation = r,
                StartDay = visibleStart.Day,
                Span = span,
                Label = label,
                CssClass = UiDisplayHelper.ReservationTimelineClass(r.Status)
            };
        }
    }

    private static IEnumerable<TimelineBarVm> BuildRegistrationBars(Vehicle vehicle, DateOnly month)
    {
        if (vehicle.RegistrationDueDate is not { } regDate || regDate.Month != month.Month)
            yield break;

        var occurrence = VehicleRegistrationHelper.OnYear(regDate, month.Year);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var isPast = occurrence < today;
        yield return new TimelineBarVm
        {
            StartDay = occurrence.Day,
            Span = 1,
            Label = isPast ? "Registracija ✓" : "Registracija",
            CssClass = isPast ? "registration registration-past" : "registration",
            IsRegistration = true
        };
    }
}
