using CarRent.Model.Entities;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public static class ReservationAvailabilityHelper
{
    private static readonly HashSet<ReservationStatus> BlockingStatuses =
    [
        ReservationStatus.Draft,
        ReservationStatus.Confirmed,
        ReservationStatus.Active
    ];

    public static bool IsBlockingStatus(ReservationStatus status) => BlockingStatuses.Contains(status);

    public static bool DatesOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        => TimelineSlotHelper.RangesOverlap(
            DateOnly.FromDateTime(startA),
            DateOnly.FromDateTime(endA),
            DateOnly.FromDateTime(startB),
            DateOnly.FromDateTime(endB));

    public static bool ConflictsWith(Reservation candidate, Reservation existing)
    {
        if (candidate.Id == existing.Id)
            return false;

        if (!IsBlockingStatus(existing.Status))
            return false;

        return DatesOverlap(candidate.StartDate, candidate.EndDate, existing.StartDate, existing.EndDate);
    }

    public static Reservation? FindConflict(
        Reservation candidate,
        IEnumerable<Reservation> existingReservations)
        => existingReservations.FirstOrDefault(r => ConflictsWith(candidate, r));
}
