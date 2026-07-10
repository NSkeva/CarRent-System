using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.Repositories;
using CarRent.Web.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services;

public sealed record FleetClientReservationSubmissionResult(
    bool Success,
    int? ReservationId,
    string Message);

public sealed class FleetClientReservationSubmissionService(
    CarRentDbContext db,
    ReservationRepository reservations,
    IFleetNotificationSender notificationSender,
    FleetNotificationDispatchService dispatchService,
    ILogger<FleetClientReservationSubmissionService> logger)
{
    public async Task<FleetClientReservationSubmissionResult> SubmitAsync(
        FleetClientChatSession session,
        CancellationToken ct = default)
    {
        if (session.CreatedReservationId is not null)
        {
            return new FleetClientReservationSubmissionResult(
                true,
                session.CreatedReservationId,
                $"Rezervacija #{session.CreatedReservationId} je već upisana u sustav. Naš tim će vas kontaktirati na {session.CustomerEmail}.");
        }

        var validationError = ValidateSession(session);
        if (validationError is not null)
            return new FleetClientReservationSubmissionResult(false, null, validationError);

        var vehicle = await db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == session.SelectedVehicleId, ct);

        if (vehicle is null || !vehicle.IsActive)
            return new FleetClientReservationSubmissionResult(false, null, "Odabrano vozilo više nije dostupno. Odaberite drugi model.");

        var start = session.RequestedFrom!.Value.ToDateTime(TimeOnly.MinValue);
        var end = session.RequestedTo!.Value.ToDateTime(TimeOnly.MinValue);
        if (end <= start)
            end = start.AddDays(1);

        var conflict = await reservations.FindSchedulingConflictAsync(vehicle.Id, start, end);
        if (conflict is not null)
        {
            return new FleetClientReservationSubmissionResult(
                false,
                null,
                $"Nažalost, {vehicle.Brand} {vehicle.Model} više nije slobodan u tom terminu. Probajte druge datume ili model.");
        }

        var customer = await FindOrCreateCustomerAsync(session, ct);
        var days = RentalDayCount(session.RequestedFrom.Value, session.RequestedTo.Value);
        var pickup = session.PickupLocation ?? LocationType.MainOffice;

        var reservation = new Reservation
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            StartDate = start,
            EndDate = end,
            PickupLocation = pickup,
            DropoffLocation = pickup,
            Status = ReservationStatus.Draft,
            BasePrice = days * vehicle.DailyPrice,
            CreatedAt = DateTime.UtcNow
        };

        FleetLifecycleRules.ApplyReservationLifecycle(reservation);
        await reservations.AddAsync(reservation);
        session.CreatedReservationId = reservation.Id;

        await NotifyTeamAsync(reservation, customer, vehicle, session, days, ct);

        try
        {
            await dispatchService.ProcessPendingAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Odmah slanje email obavijesti nije uspjelo; poruka ostaje u outboxu.");
        }

        logger.LogInformation(
            "AI asistent kreirao rezervaciju #{ReservationId} za {Email}",
            reservation.Id,
            session.CustomerEmail);

        return new FleetClientReservationSubmissionResult(
            true,
            reservation.Id,
            $"""
            ✅ Rezervacija #{reservation.Id} je upisana u sustav (status: Nacrt — tim je potvrđuje).

            Vozilo: {vehicle.Brand} {vehicle.Model}
            Termin: {FleetAiDateParser.FormatRange(session.RequestedFrom.Value, session.RequestedTo.Value)}
            Procjena: ~{reservation.BasePrice:N0} EUR

            Obavijest je poslana našem timu na email. SMS na mobitel trenutno nije podržan — javljamo vam se na {session.CustomerEmail} ili {session.CustomerPhone} kad potvrdimo rezervaciju.
            """);
    }

    private static string? ValidateSession(FleetClientChatSession session)
    {
        if (session.SelectedVehicleId is null)
            return "Nedostaje odabrano vozilo. Prvo odaberite model s ponude.";
        if (session.RequestedFrom is null || session.RequestedTo is null)
            return "Nedostaju datumi najma.";
        if (string.IsNullOrWhiteSpace(session.CustomerName))
            return "Nedostaje ime za rezervaciju.";
        if (string.IsNullOrWhiteSpace(session.CustomerEmail))
            return "Nedostaje email za potvrdu.";
        if (string.IsNullOrWhiteSpace(session.CustomerPhone))
            return "Nedostaje broj mobitela za kontakt.";
        return null;
    }

    private async Task<Customer> FindOrCreateCustomerAsync(FleetClientChatSession session, CancellationToken ct)
    {
        var email = session.CustomerEmail!.Trim();
        var existing = await db.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower(), ct);

        if (existing is not null)
        {
            if (string.IsNullOrWhiteSpace(existing.Phone) && !string.IsNullOrWhiteSpace(session.CustomerPhone))
            {
                existing.Phone = session.CustomerPhone.Trim();
                await db.SaveChangesAsync(ct);
            }

            return existing;
        }

        var (firstName, lastName) = SplitName(session.CustomerName!);
        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = session.CustomerPhone!.Trim(),
            DateOfBirth = new DateTime(1990, 1, 1),
            DriverLicenseNumber = $"AI-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CreatedAt = DateTime.UtcNow
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        return customer;
    }

    private async Task NotifyTeamAsync(
        Reservation reservation,
        Customer customer,
        Vehicle vehicle,
        FleetClientChatSession session,
        int days,
        CancellationToken ct)
    {
        var pickup = session.PickupLocation is null
            ? "Glavna poslovnica"
            : FleetAiIntentParser.FormatLocation(session.PickupLocation.Value);

        await notificationSender.SendAsync(new FleetNotificationMessage
        {
            NotificationType = FleetNotificationTypes.ClientAssistantBooking,
            Subject = $"AI asistent — nova rezervacija #{reservation.Id}",
            Body = $"""
                Nova rezervacija putem klijentskog AI asistenta.

                Rezervacija: #{reservation.Id} (Nacrt)
                Vozilo: {vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber})
                Termin: {reservation.StartDate:dd.MM.yyyy} – {reservation.EndDate:dd.MM.yyyy} ({days} dana)
                Lokacija preuzimanja: {pickup}
                Cijena (baza): {reservation.BasePrice:N0} EUR

                Kupac: {customer.FirstName} {customer.LastName}
                Email: {customer.Email}
                Telefon: {customer.Phone}

                Potvrdite u aplikaciji: Rezervacije → #{reservation.Id}
                """,
            DedupKey = $"client-ai-booking:{reservation.Id}",
            RelatedEntityType = nameof(Reservation),
            RelatedEntityId = reservation.Id
        }, ct);
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return ("Klijent", "AI");
        if (parts.Length == 1)
            return (parts[0], "-");
        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static int RentalDayCount(DateOnly from, DateOnly to)
    {
        var days = to.ToDateTime(TimeOnly.MinValue).Subtract(from.ToDateTime(TimeOnly.MinValue)).Days;
        return Math.Max(1, days);
    }
}
