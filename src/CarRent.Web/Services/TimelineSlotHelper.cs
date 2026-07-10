namespace CarRent.Web.Services;

/// <summary>
/// Poludnevna mreža: svaki dan = AM + PM. Višednevna najam zauzima PM početka do AM završetka,
/// tako da isti dan može imati povrat (AM) i novo preuzimanje (PM) bez preklapanja.
/// </summary>
public static class TimelineSlotHelper
{
    public const int SlotsPerDay = 2;
    public const int HalfCellPx = 22;

    public static (int start, int end) GetOccupiedGlobalSlots(DateOnly start, DateOnly end)
    {
        if (end < start)
            return (0, -1);

        var startDay = start.DayNumber;
        var endDay = end.DayNumber;

        if (startDay == endDay)
            return (startDay * SlotsPerDay, startDay * SlotsPerDay + 1);

        return (startDay * SlotsPerDay + 1, endDay * SlotsPerDay);
    }

    public static bool RangesOverlap(DateOnly startA, DateOnly endA, DateOnly startB, DateOnly endB)
    {
        var (a0, a1) = GetOccupiedGlobalSlots(startA, endA);
        var (b0, b1) = GetOccupiedGlobalSlots(startB, endB);
        return a0 <= b1 && b0 <= a1;
    }

    public static (int startSlot, int spanSlots)? GetVisibleLayout(DateOnly start, DateOnly end, DateOnly month)
    {
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var monthStart = new DateOnly(month.Year, month.Month, 1);
        var monthEnd = new DateOnly(month.Year, month.Month, daysInMonth);

        var (globalStart, globalEnd) = GetOccupiedGlobalSlots(start, end);
        var monthGlobalStart = monthStart.DayNumber * SlotsPerDay;
        var monthGlobalEnd = monthEnd.DayNumber * SlotsPerDay + 1;

        if (globalEnd < monthGlobalStart || globalStart > monthGlobalEnd)
            return null;

        var visStart = Math.Max(globalStart, monthGlobalStart);
        var visEnd = Math.Min(globalEnd, monthGlobalEnd);
        if (visEnd < visStart)
            return null;

        var startSlot = visStart - monthGlobalStart + 1;
        var spanSlots = visEnd - visStart + 1;
        return (startSlot, spanSlots);
    }
}
