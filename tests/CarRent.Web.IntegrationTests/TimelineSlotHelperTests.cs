using CarRent.Web.Services;
using FluentAssertions;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class TimelineSlotHelperTests
{
    [Fact]
    public void Same_day_turnaround_uses_adjacent_half_day_slots()
    {
        var month = new DateOnly(2026, 6, 1);
        var returning = new DateOnly(2026, 6, 10);
        var turnaround = new DateOnly(2026, 6, 15);
        var departing = new DateOnly(2026, 6, 15);
        var nextTripEnd = new DateOnly(2026, 6, 20);

        var first = TimelineSlotHelper.GetVisibleLayout(returning, turnaround, month);
        var second = TimelineSlotHelper.GetVisibleLayout(departing, nextTripEnd, month);

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        (first!.Value.startSlot + first.Value.spanSlots).Should().Be(second!.Value.startSlot);
    }

    [Fact]
    public void Multi_day_reservation_spans_pm_start_through_am_end()
    {
        var month = new DateOnly(2026, 6, 1);
        var layout = TimelineSlotHelper.GetVisibleLayout(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 15), month);

        layout.Should().NotBeNull();
        layout!.Value.spanSlots.Should().Be(10);
    }
}
