using CarRent.Model.Entities;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Services;

public static class TimelineLayoutHelper
{
    public static IReadOnlyList<TimelineBarVm> BuildBars(IEnumerable<Reservation> reservations, DateOnly month)
    {
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var monthStart = new DateOnly(month.Year, month.Month, 1);
        var monthEnd = new DateOnly(month.Year, month.Month, daysInMonth);
        var bars = new List<TimelineBarVm>();

        foreach (var r in reservations)
        {
            var start = DateOnly.FromDateTime(r.StartDate);
            var end = DateOnly.FromDateTime(r.EndDate);
            if (end < monthStart || start > monthEnd) continue;

            var visibleStart = start < monthStart ? monthStart : start;
            var visibleEnd = end > monthEnd ? monthEnd : end;
            var span = visibleEnd.Day - visibleStart.Day + 1;
            if (span < 1) continue;

            bars.Add(new TimelineBarVm
            {
                Reservation = r,
                StartDay = visibleStart.Day,
                Span = span
            });
        }

        return bars;
    }
}
