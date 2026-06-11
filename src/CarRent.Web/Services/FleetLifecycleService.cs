using CarRent.DAL;
using CarRent.Model.Enums;
using CarRent.Web.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Services;

public sealed record FleetLifecycleSyncResult(
    int ReservationUpdates,
    int ServiceUpdates,
    int VehicleHoldUpdates,
    int NotificationsPrepared)
{
    public int TotalUpdates => ReservationUpdates + ServiceUpdates + VehicleHoldUpdates;
}

public sealed class FleetLifecycleService(
    CarRentDbContext db,
    FleetNotificationService notifications,
    IOptions<FleetLifecycleOptions> options,
    ILogger<FleetLifecycleService> logger)
{
    public async Task<FleetLifecycleSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var opts = options.Value;
        var reservationUpdates = 0;
        var serviceUpdates = 0;
        var vehicleHoldUpdates = 0;

        var reservations = await db.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            var change = FleetLifecycleRules.EvaluateReservation(reservation, opts, now);

            if (change.IsDraftReminder)
            {
                await notifications.NotifyDraftReminderAsync(
                    reservation.Id,
                    reservation.Customer?.Email ?? string.Empty,
                    reservation.CreatedAt.AddDays(opts.DraftExpiryDays),
                    cancellationToken);
                continue;
            }

            if (!change.HasStatusChange && !change.SuggestMileageUpdate)
                continue;

            if (change.NewStatus is not null && change.NewStatus != reservation.Status)
            {
                logger.LogInformation(
                    "Rezervacija #{Id}: {From} → {To}",
                    reservation.Id,
                    reservation.Status,
                    change.NewStatus);

                reservation.Status = change.NewStatus.Value;
                reservationUpdates++;

                if (change.IsNoShowCancellation)
                {
                    await notifications.NotifyNoShowCancelledAsync(
                        reservation.Id,
                        reservation.Customer?.Email,
                        cancellationToken);
                }

                if (change.IsDraftExpiry)
                    await notifications.NotifyDraftExpiredAsync(reservation.Id, cancellationToken);
            }

            if (change.SuggestMileageUpdate && !reservation.MileageUpdateSuggested)
            {
                reservation.MileageUpdateSuggested = true;
                reservationUpdates++;

                var reg = reservation.Vehicle?.RegistrationNumber ?? $"#{reservation.VehicleId}";
                await notifications.NotifyMileageUpdateSuggestedAsync(
                    reservation.Id,
                    reservation.VehicleId,
                    reg,
                    cancellationToken);
            }
        }

        var services = await db.ServiceRecords
            .Where(s => s.Status != ServiceStatus.Cancelled && s.Status != ServiceStatus.Completed)
            .ToListAsync(cancellationToken);

        foreach (var service in services)
        {
            var next = FleetLifecycleRules.ResolveServiceStatus(service.Status, service.ServiceDate, today);
            if (next is null || next == service.Status)
                continue;

            logger.LogInformation("Servis #{Id}: {From} → {To}", service.Id, service.Status, next);
            service.Status = next.Value;
            serviceUpdates++;
        }

        vehicleHoldUpdates = await SyncVehicleServiceHoldsAsync(cancellationToken);

        if (reservationUpdates + serviceUpdates + vehicleHoldUpdates > 0)
            await db.SaveChangesAsync(cancellationToken);

        var notificationsPrepared = await notifications.PrepareDailyNotificationsAsync(now, cancellationToken);

        return new FleetLifecycleSyncResult(
            reservationUpdates,
            serviceUpdates,
            vehicleHoldUpdates,
            notificationsPrepared);
    }

    private async Task<int> SyncVehicleServiceHoldsAsync(CancellationToken cancellationToken)
    {
        var vehiclesInService = await db.ServiceRecords
            .Where(s => s.Status == ServiceStatus.InProgress)
            .Select(s => s.VehicleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var inServiceSet = vehiclesInService.ToHashSet();
        var vehicles = await db.Vehicles.ToListAsync(cancellationToken);
        var updates = 0;

        foreach (var vehicle in vehicles)
        {
            var shouldBlock = inServiceSet.Contains(vehicle.Id);

            if (shouldBlock && !vehicle.BlockedByService)
            {
                vehicle.IsActiveBeforeServiceBlock = vehicle.IsActive;
                vehicle.BlockedByService = true;
                vehicle.IsActive = false;
                updates++;
            }
            else if (!shouldBlock && vehicle.BlockedByService)
            {
                vehicle.IsActive = vehicle.IsActiveBeforeServiceBlock;
                vehicle.BlockedByService = false;
                updates++;
            }
        }

        return updates;
    }
}
