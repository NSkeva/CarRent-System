using CarRent.Model.Entities;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public sealed class ReservationLifecycleChange
{
    public ReservationStatus? NewStatus { get; init; }
    public bool SuggestMileageUpdate { get; init; }
    public bool IsNoShowCancellation { get; init; }
    public bool IsDraftExpiry { get; init; }
    public bool IsDraftReminder { get; init; }

    public bool HasStatusChange => NewStatus is not null;
}

public static class FleetLifecycleRules
{
    public static ReservationLifecycleChange EvaluateReservation(
        Reservation reservation,
        FleetLifecycleOptions options,
        DateTime now)
    {
        if (reservation.Status == ReservationStatus.Draft)
            return EvaluateDraft(reservation, options, now);

        // Terminalni statusi — nikad se ne mijenjaju automatski.
        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Completed)
            return new ReservationLifecycleChange();

        var next = ResolveReservationStatus(
            reservation.Status,
            reservation.StartDate,
            reservation.EndDate,
            now.Date);

        if (next == ReservationStatus.Completed)
        {
            return new ReservationLifecycleChange
            {
                NewStatus = ReservationStatus.Completed,
                SuggestMileageUpdate = true
            };
        }

        if (next == ReservationStatus.Cancelled && reservation.Status == ReservationStatus.Confirmed)
        {
            return new ReservationLifecycleChange
            {
                NewStatus = ReservationStatus.Cancelled,
                IsNoShowCancellation = true
            };
        }

        var noShow = ResolveNoShow(
            reservation.Status,
            reservation.StartDate,
            reservation.EndDate,
            now,
            options.NoShowHoursAfterStart);

        if (noShow == ReservationStatus.Cancelled)
        {
            return new ReservationLifecycleChange
            {
                NewStatus = ReservationStatus.Cancelled,
                IsNoShowCancellation = true
            };
        }

        if (next is not null)
            return new ReservationLifecycleChange { NewStatus = next };

        return new ReservationLifecycleChange();
    }

    /// <summary>
    /// No-show: potvrđena rezervacija bez preuzimanja 24 h nakon početka, dok još traje period.
    /// </summary>
    public static ReservationStatus? ResolveNoShow(
        ReservationStatus current,
        DateTime start,
        DateTime end,
        DateTime now,
        int noShowHoursAfterStart)
    {
        if (current != ReservationStatus.Confirmed)
            return null;

        if (now.Date > end.Date)
            return null;

        return now > start.AddHours(noShowHoursAfterStart)
            ? ReservationStatus.Cancelled
            : null;
    }

    private static ReservationLifecycleChange EvaluateDraft(
        Reservation reservation,
        FleetLifecycleOptions options,
        DateTime now)
    {
        var ageDays = (now - reservation.CreatedAt).TotalDays;
        if (ageDays >= options.DraftExpiryDays)
        {
            return new ReservationLifecycleChange
            {
                NewStatus = ReservationStatus.Cancelled,
                IsDraftExpiry = true
            };
        }

        var reminderFromDay = options.DraftExpiryDays - options.DraftReminderDaysBeforeExpiry;
        if (ageDays >= reminderFromDay)
            return new ReservationLifecycleChange { IsDraftReminder = true };

        return new ReservationLifecycleChange();
    }

    /// <summary>
    /// Vraća novi status rezervacije ili null ako se ne mijenja.
    /// Završena samo iz Aktivne; otkazana ostaje otkazana.
    /// </summary>
    public static ReservationStatus? ResolveReservationStatus(
        ReservationStatus current,
        DateTime start,
        DateTime end,
        DateTime today)
    {
        if (current is ReservationStatus.Cancelled or ReservationStatus.Completed or ReservationStatus.Draft)
            return null;

        var startDay = start.Date;
        var endDay = end.Date;
        var todayDay = today.Date;

        if (endDay < todayDay)
        {
            return current switch
            {
                ReservationStatus.Active => ReservationStatus.Completed,
                ReservationStatus.Confirmed => ReservationStatus.Cancelled,
                _ => null
            };
        }

        if (startDay <= todayDay && todayDay <= endDay)
            return current == ReservationStatus.Active ? null : ReservationStatus.Active;

        if (startDay > todayDay && current == ReservationStatus.Active)
            return ReservationStatus.Confirmed;

        return null;
    }

    public static ServiceStatus? ResolveServiceStatus(
        ServiceStatus current,
        DateTime serviceDate,
        DateTime today)
    {
        // Otkazano i završeno — terminalno.
        if (current is ServiceStatus.Cancelled or ServiceStatus.Completed)
            return null;

        var serviceDay = serviceDate.Date;
        var todayDay = today.Date;

        if (current == ServiceStatus.InProgress && serviceDay < todayDay)
            return ServiceStatus.Completed;

        if (current == ServiceStatus.Planned && serviceDay <= todayDay)
            return ServiceStatus.InProgress;

        return null;
    }

    public static void ApplyReservationLifecycle(
        Reservation reservation,
        FleetLifecycleOptions? options = null,
        DateTime? now = null)
    {
        options ??= new FleetLifecycleOptions();
        now ??= DateTime.Now;

        var change = EvaluateReservation(reservation, options, now.Value);
        if (change.NewStatus is not null)
            reservation.Status = change.NewStatus.Value;

        if (change.SuggestMileageUpdate)
            reservation.MileageUpdateSuggested = true;
    }

    public static void ApplyServiceLifecycle(ServiceRecord service, DateTime? today = null)
    {
        var next = ResolveServiceStatus(
            service.Status,
            service.ServiceDate,
            today ?? DateTime.Today);

        if (next is not null)
            service.Status = next.Value;
    }
}
