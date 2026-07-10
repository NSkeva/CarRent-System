using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Services.Notifications;

public sealed class FleetNotificationService(
    CarRentDbContext db,
    IFleetNotificationSender sender,
    IOptions<FleetLifecycleOptions> lifecycleOptions)
{
    public async Task<int> PrepareDailyNotificationsAsync(DateTime today, CancellationToken cancellationToken = default)
    {
        var prepared = 0;
        var opts = lifecycleOptions.Value;
        var todayDate = today.Date;

        var vehicles = await db.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var vehicle in vehicles.Where(v => v.RegistrationDueDate is not null))
        {
            var regDate = VehicleRegistrationHelper.OnYear(vehicle.RegistrationDueDate!.Value, today.Year);
            if ((regDate.ToDateTime(TimeOnly.MinValue) - todayDate).TotalDays != opts.RegistrationNotificationDaysAhead)
                continue;

            await sender.SendAsync(new FleetNotificationMessage
            {
                NotificationType = FleetNotificationTypes.RegistrationDue,
                Subject = $"Registracija vozila {vehicle.RegistrationNumber} za {opts.RegistrationNotificationDaysAhead} dana",
                Body = $"Vozilo {vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber}) ima registraciju {regDate:dd.MM.yyyy}.",
                DedupKey = $"reg:{vehicle.Id}:{today:yyyy-MM-dd}",
                RelatedEntityType = nameof(Vehicle),
                RelatedEntityId = vehicle.Id
            }, cancellationToken);
            prepared++;
        }

        var tomorrow = todayDate.AddDays(1);
        var servicesTomorrow = await db.ServiceRecords
            .Include(s => s.Vehicle)
            .AsNoTracking()
            .Where(s => s.ServiceDate.Date == tomorrow
                        && s.Status != ServiceStatus.Cancelled
                        && s.Status != ServiceStatus.Completed)
            .ToListAsync(cancellationToken);

        foreach (var service in servicesTomorrow)
        {
            var vehicle = service.Vehicle;
            var label = vehicle is null
                ? $"vozilo #{service.VehicleId}"
                : $"{vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber})";

            await sender.SendAsync(new FleetNotificationMessage
            {
                NotificationType = FleetNotificationTypes.ServiceTomorrow,
                Subject = $"Servis sutra: {label}",
                Body = $"Planirani servis {tomorrow:dd.MM.yyyy}: {service.Description}",
                DedupKey = $"svc-tmr:{service.Id}:{today:yyyy-MM-dd}",
                RelatedEntityType = nameof(ServiceRecord),
                RelatedEntityId = service.Id
            }, cancellationToken);
            prepared++;
        }

        var returnsToday = await db.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .AsNoTracking()
            .Where(r => r.EndDate.Date == todayDate && r.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var reservation in returnsToday)
        {
            var customer = reservation.Customer is null
                ? "kupac"
                : $"{reservation.Customer.FirstName} {reservation.Customer.LastName}";

            await sender.SendAsync(new FleetNotificationMessage
            {
                NotificationType = FleetNotificationTypes.ReturnToday,
                Subject = $"Povrat vozila danas — rezervacija #{reservation.Id}",
                Body = $"{customer} vraća {reservation.Vehicle?.RegistrationNumber ?? "vozilo"} na lokaciji {reservation.DropoffLocation}.",
                DedupKey = $"ret-today:{reservation.Id}:{today:yyyy-MM-dd}",
                RelatedEntityType = nameof(Reservation),
                RelatedEntityId = reservation.Id
            }, cancellationToken);
            prepared++;
        }

        return prepared;
    }

    public Task NotifyDraftReminderAsync(int reservationId, string recipientEmail, DateTime expiryDate, CancellationToken cancellationToken = default)
        => sender.SendAsync(new FleetNotificationMessage
        {
            NotificationType = FleetNotificationTypes.DraftExpiryReminder,
            Subject = $"Nacrt rezervacije #{reservationId} ističe uskoro",
            Body = $"Nacrt rezervacije #{reservationId} bit će automatski otkazan {expiryDate:dd.MM.yyyy} ako se ne potvrdi." +
                   (string.IsNullOrWhiteSpace(recipientEmail) ? "" : $" (kupac: {recipientEmail})"),
            DedupKey = $"draft-reminder:{reservationId}:{expiryDate:yyyy-MM-dd}",
            RelatedEntityType = nameof(Reservation),
            RelatedEntityId = reservationId
        }, cancellationToken);

    public Task NotifyDraftExpiredAsync(int reservationId, CancellationToken cancellationToken = default)
        => sender.SendAsync(new FleetNotificationMessage
        {
            NotificationType = FleetNotificationTypes.DraftExpired,
            Subject = $"Nacrt rezervacije #{reservationId} je otkazan",
            Body = $"Nacrt rezervacije #{reservationId} automatski je otkazan jer nije potvrđen na vrijeme.",
            DedupKey = $"draft-expired:{reservationId}:{DateTime.UtcNow:yyyy-MM-dd}",
            RelatedEntityType = nameof(Reservation),
            RelatedEntityId = reservationId
        }, cancellationToken);

    public Task NotifyNoShowCancelledAsync(int reservationId, string? recipientEmail, CancellationToken cancellationToken = default)
        => sender.SendAsync(new FleetNotificationMessage
        {
            NotificationType = FleetNotificationTypes.NoShowCancelled,
            Subject = $"Rezervacija #{reservationId} otkazana (no-show)",
            Body = $"Rezervacija #{reservationId} automatski je otkazana jer vozilo nije preuzeto u roku od 24 sata." +
                   (string.IsNullOrWhiteSpace(recipientEmail) ? "" : $" (kupac: {recipientEmail})"),
            DedupKey = $"no-show:{reservationId}:{DateTime.UtcNow:yyyy-MM-dd}",
            RelatedEntityType = nameof(Reservation),
            RelatedEntityId = reservationId
        }, cancellationToken);

    public Task NotifyMileageUpdateSuggestedAsync(int reservationId, int vehicleId, string registration, CancellationToken cancellationToken = default)
        => sender.SendAsync(new FleetNotificationMessage
        {
            NotificationType = FleetNotificationTypes.MileageUpdateSuggested,
            Subject = $"Ažuriraj kilometražu — {registration}",
            Body = $"Rezervacija #{reservationId} je završena. Predloženo je ažuriranje kilometraže vozila {registration}.",
            DedupKey = $"mileage:{reservationId}",
            RelatedEntityType = nameof(Reservation),
            RelatedEntityId = reservationId
        }, cancellationToken);
}
