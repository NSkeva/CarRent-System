using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Services;

public static class TimelineLayoutHelper
{
    public static IReadOnlyList<TimelineBarVm> BuildBars(Vehicle vehicle, IEnumerable<Reservation> reservations, DateOnly month)
    {
        var bars = BuildReservationBars(reservations, month).ToList();
        bars.AddRange(BuildRegistrationBars(vehicle, month));
        return bars.OrderBy(b => b.StartSlot).ToList();
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

            var layout = TimelineSlotHelper.GetVisibleLayout(start, end, month);
            if (layout is null) continue;

            var label = r.Customer is null ? $"#{r.Id}" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            yield return new TimelineBarVm
            {
                Reservation = r,
                ReservationId = r.Id,
                VehicleId = r.VehicleId,
                ActualStart = start,
                ActualEnd = end,
                IsEditable = IsReservationEditable(r.Status),
                StartSlot = layout.Value.startSlot,
                SpanSlots = layout.Value.spanSlots,
                Label = label,
                CssClass = UiDisplayHelper.ReservationTimelineClass(r.Status),
                DetailsUrl = $"/rezervacije/pregled/{r.Id}"
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
        var regSlot = (occurrence.DayNumber - month.DayNumber) * TimelineSlotHelper.SlotsPerDay + 1;
        yield return new TimelineBarVm
        {
            StartSlot = regSlot,
            SpanSlots = 1,
            Label = isPast ? "Registracija ✓" : "Registracija",
            CssClass = isPast ? "registration registration-past" : "registration",
            IsRegistration = true
        };
    }

    private static bool IsReservationEditable(ReservationStatus status)
        => status is ReservationStatus.Draft or ReservationStatus.Confirmed or ReservationStatus.Active;
}
