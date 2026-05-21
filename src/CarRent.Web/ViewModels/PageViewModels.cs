using CarRent.Model.Entities;
using CarRent.Model.Enums;

namespace CarRent.Web.ViewModels;

public sealed class HomeIndexVm
{
    public int TotalVehicles { get; init; }
    public int ActiveReservations { get; init; }
    public int Branches { get; init; }
    public int ServicesDueSoon { get; init; }
    public required IReadOnlyList<HomeQuickLinkVm> QuickLinks { get; init; }
    public required IReadOnlyList<DailyReservationVm> TodayDepartures { get; init; }
    public required IReadOnlyList<DailyReservationVm> TodayReturns { get; init; }
}

public sealed class HomeQuickLinkVm
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Controller { get; init; }
    public required string Action { get; init; }
}

public sealed class TimelineVm
{
    public required DateOnly Month { get; init; }
    public required string SearchText { get; init; }
    public string? VehicleType { get; init; }
    public string? ReservationStatus { get; init; }
    public IReadOnlyList<VehicleType> VehicleTypes { get; init; } = [];
    public IReadOnlyList<ReservationStatus> ReservationStatuses { get; init; } = [];
    public required IReadOnlyList<DateOnly> Days { get; init; }
    public required IReadOnlyList<TimelineRowVm> Rows { get; init; }
}

public sealed class TimelineRowVm
{
    public required Vehicle Vehicle { get; init; }
    public required IReadOnlyList<TimelineCellVm> Cells { get; init; }
    public required IReadOnlyList<TimelineBarVm> Bars { get; init; }
    public string? BranchName { get; init; }
}

public sealed class TimelineBarVm
{
    public required Reservation Reservation { get; init; }
    public int StartDay { get; init; }
    public int Span { get; init; }
}

public sealed class TimelineCellVm
{
    public required DateOnly Day { get; init; }
    public required IReadOnlyList<Reservation> Reservations { get; init; }
}

public sealed class DailyPlanVm
{
    public required DateOnly Day { get; init; }
    public required IReadOnlyList<DailyReservationVm> Returns { get; init; }
    public required IReadOnlyList<DailyReservationVm> Departures { get; init; }
}

public sealed class DailyReservationVm
{
    public required Reservation Reservation { get; init; }
    public required Vehicle Vehicle { get; init; }
    public required Customer Customer { get; init; }
}

public sealed class FleetCardVm
{
    public required Vehicle Vehicle { get; init; }
    public required string ImageUrl { get; init; }
    public required string BranchName { get; init; }
}

public sealed class BreadcrumbHelper
{
    public static string Build(params string[] items) => string.Join(" / ", items);
}
