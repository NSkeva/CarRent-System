using System.Text.Json;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public sealed class FleetClientChatConversation(
    FleetAiAvailabilityService availabilityService,
    FleetClientReservationSubmissionService submissionService,
    ILogger<FleetClientChatConversation> logger)
{
    public async Task<string> ProcessAsync(FleetClientChatSession session, string userMessage, CancellationToken ct)
    {
        FleetAiIntentParser.TryExtractContact(userMessage, session);

        if (session.Phase is FleetClientChatPhase.ReadyToConfirm or FleetClientChatPhase.Completed)
        {
            if (FleetAiIntentParser.IsAffirmative(userMessage))
            {
                session.Phase = FleetClientChatPhase.Completed;
                var result = await submissionService.SubmitAsync(session, ct);
                return result.Message;
            }

            if (FleetAiIntentParser.IsRentalNeed(userMessage))
            {
                ResetBooking(session, keepHistory: true);
            }
            else
            {
                return "Želite li potvrditi rezervaciju? Odgovorite „da” ili napišite što želite promijeniti.";
            }
        }

        if (FleetAiIntentParser.IsRentalNeed(userMessage) || session.Phase != FleetClientChatPhase.Idle)
            return await HandleBookingFlowAsync(session, userMessage, ct);

        return await HandleGeneralAsync(session, userMessage, ct);
    }

    private async Task<string> HandleBookingFlowAsync(
        FleetClientChatSession session,
        string userMessage,
        CancellationToken ct)
    {
        if (session.Phase is FleetClientChatPhase.Idle or FleetClientChatPhase.Browsing)
            return await ShowAvailabilityAsync(session, userMessage, ct);

        if (session.Phase == FleetClientChatPhase.SelectingVehicle)
            return await SelectVehicleAsync(session, userMessage, ct);

        if (session.Phase == FleetClientChatPhase.CollectingContact)
            return ContinueContactCollection(session);

        return await ShowAvailabilityAsync(session, userMessage, ct);
    }

    private async Task<string> ShowAvailabilityAsync(
        FleetClientChatSession session,
        string userMessage,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var (from, to) = FleetAiDateParser.TryParseRange(userMessage);
        from ??= session.RequestedFrom;
        to ??= session.RequestedTo;

        if (from is null && FleetAiIntentParser.IsRentalNeed(userMessage))
        {
            if (userMessage.Contains("vikend", StringComparison.OrdinalIgnoreCase))
            {
                (from, to) = FleetAiDateParser.TryParseRange("ovaj vikend");
            }
            else
            {
                from = today;
                to = today.AddDays(2);
            }
        }

        if (from is null)
            return "Super, mogu pomoći oko najma. Za koje datume vam treba vozilo? (npr. ovaj vikend ili 15.–22.7.)";

        to ??= from;
        var typeFilter = FleetAiDateParser.TryParseVehicleType(userMessage);
        var result = await availabilityService.GetAvailabilityAsync(from.Value, to.Value, typeFilter, ct);

        session.RequestedFrom = from;
        session.RequestedTo = to;
        session.LastOffered = result.FreeVehicles.Select(FleetOfferedVehicle.From).ToList();
        session.Phase = FleetClientChatPhase.SelectingVehicle;
        session.SelectedVehicleId = null;
        session.SelectedVehicleLabel = null;

        if (session.LastOffered.Count == 0)
        {
            session.Phase = FleetClientChatPhase.Browsing;
            return FleetAiAvailabilityService.FormatClientAvailabilityReply(result, typeFilter) +
                   "\n\nŽelite li probati druge datume?";
        }

        var period = FleetAiDateParser.FormatRange(from.Value, to.Value);
        var lines = session.LastOffered.Take(6).Select((v, i) => $"{i + 1}. {v.Label} — {v.DailyPrice:N0} EUR/dan");

        return $"Za {period} imamo {session.LastOffered.Count} slobodnih vozila:\n" +
               string.Join("\n", lines) +
               (session.LastOffered.Count > 6 ? $"\n… i još {session.LastOffered.Count - 6}." : "") +
               "\n\nKoji model želite? Možete reći npr. „Golf” ili „taj prvi”, pa dodati trajanje (npr. „7 dana”).";
    }

    private async Task<string> SelectVehicleAsync(
        FleetClientChatSession session,
        string userMessage,
        CancellationToken ct)
    {
        var vehicle = FleetAiVehicleMatcher.Match(userMessage, session);
        var rentalDays = FleetAiIntentParser.TryParseRentalDays(userMessage);
        var (parsedFrom, parsedTo) = FleetAiDateParser.TryParseRange(userMessage);

        if (parsedFrom is not null)
        {
            session.RequestedFrom = parsedFrom;
            session.RequestedTo = parsedTo ?? parsedFrom;
        }

        if (rentalDays is not null && session.RequestedFrom is not null)
            session.RequestedTo = session.RequestedFrom.Value.AddDays(rentalDays.Value);

        if (vehicle is null && session.SelectedVehicleId is not null && FleetAiIntentParser.ReferencesPreviousChoice(userMessage))
            vehicle = session.LastOffered.FirstOrDefault(v => v.Id == session.SelectedVehicleId);

        if (vehicle is null)
        {
            if (rentalDays is not null && session.RequestedFrom is not null)
                return "Razumijem trajanje, ali koji model želite? Izaberite s liste (npr. Golf ili „prvi”).";

            return "Nisam siguran koji model mislite. Navedite naziv s liste (npr. Golf, Passat) ili „prvi”.";
        }

        if (session.RequestedFrom is null || session.RequestedTo is null)
            return $"Odlično, {vehicle.Label}. Na koliko dana želite najam? (npr. „7 dana”)";

        var slotCheck = await availabilityService.GetAvailabilityAsync(
            session.RequestedFrom.Value,
            session.RequestedTo.Value,
            ct: ct);

        if (slotCheck.FreeVehicles.All(v => v.Id != vehicle.Id))
        {
            return $"{vehicle.Label} nije slobodan za {FleetAiDateParser.FormatRange(session.RequestedFrom.Value, session.RequestedTo.Value)}. " +
                   "Želite li drugi model ili druge datume?";
        }

        session.SelectedVehicleId = vehicle.Id;
        session.SelectedVehicleLabel = vehicle.Label;
        session.SelectedDailyPrice = vehicle.DailyPrice;
        session.Phase = FleetClientChatPhase.CollectingContact;

        var days = RentalDayCount(session.RequestedFrom.Value, session.RequestedTo.Value);
        var estimated = days * vehicle.DailyPrice;

        var intro = $"Super — {vehicle.Label} za {FleetAiDateParser.FormatRange(session.RequestedFrom.Value, session.RequestedTo.Value)} " +
                    $"(oko {days} dana, procjena ~{estimated:N0} EUR).\n\n";

        var missing = FleetAiIntentParser.NextMissingContactField(session);
        return missing is null
            ? intro + BuildConfirmationSummary(session) + "\n\nŽelite li potvrditi? (da/ne)"
            : intro + "Za dovršetak rezervacije trebam još par podataka.\n" + missing;
    }

    private string ContinueContactCollection(FleetClientChatSession session)
    {
        var missing = FleetAiIntentParser.NextMissingContactField(session);
        if (missing is not null)
            return missing;

        session.Phase = FleetClientChatPhase.ReadyToConfirm;
        return BuildConfirmationSummary(session) + "\n\nSve izgleda dobro — želite li potvrditi rezervaciju?";
    }

    private async Task<string> HandleGeneralAsync(
        FleetClientChatSession session,
        string userMessage,
        CancellationToken ct)
    {
        var msg = userMessage.ToLowerInvariant();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        if (msg.Contains("cijen", StringComparison.Ordinal) || msg.Contains("koliko", StringComparison.Ordinal) || msg.Contains("eur", StringComparison.Ordinal))
        {
            var typeFilter = FleetAiDateParser.TryParseVehicleType(userMessage);
            var vehicles = await availabilityService.GetAvailabilityAsync(today, today, typeFilter, ct);
            if (vehicles.FreeVehicles.Count == 0)
                return "Trenutno nemam slobodnih vozila za prikaz cijena — pitajte za konkretan termin.";

            var min = vehicles.FreeVehicles.Min(v => v.DailyPrice);
            var max = vehicles.FreeVehicles.Max(v => v.DailyPrice);
            return min == max
                ? $"Dnevna cijena kreće od {min:N0} EUR. Recite mi datume pa ću predložiti konkretne modele."
                : $"Cijene su od {min:N0} do {max:N0} EUR/dan. Za koji termin trebate vozilo?";
        }

        if (msg.Contains("pozdrav", StringComparison.Ordinal) || msg.Contains("bok", StringComparison.Ordinal) || msg.Contains("hello", StringComparison.Ordinal))
            return "Bok! Recite mi jednostavno što trebate — npr. „Trebam auto ovaj vikend” — i vodim vas do rezervacije.";

        return "Mogu voditi cijelu rezervaciju u razgovoru. Probajte: „Trebam auto ovaj vikend” ili „Ima li kombija sutra?”.";
    }

    private static int RentalDayCount(DateOnly from, DateOnly to)
    {
        var days = to.ToDateTime(TimeOnly.MinValue).Subtract(from.ToDateTime(TimeOnly.MinValue)).Days;
        return Math.Max(1, days);
    }

    private static string BuildConfirmationSummary(FleetClientChatSession session)
    {
        var days = session.RequestedFrom is not null && session.RequestedTo is not null
            ? RentalDayCount(session.RequestedFrom.Value, session.RequestedTo.Value)
            : 1;
        var total = (session.SelectedDailyPrice ?? 0) * days;

        return $"""
            📋 Sažetak rezervacije:
            Vozilo: {session.SelectedVehicleLabel ?? "—"}
            Termin: {FleetAiDateParser.FormatRange(session.RequestedFrom ?? DateOnly.FromDateTime(DateTime.UtcNow), session.RequestedTo ?? DateOnly.FromDateTime(DateTime.UtcNow))}
            Trajanje: ~{days} dana
            Procjena: ~{total:N0} EUR
            Ime: {session.CustomerName}
            Email: {session.CustomerEmail}
            Telefon: {session.CustomerPhone}
            Preuzimanje: {(session.PickupLocation is null ? "—" : FleetAiIntentParser.FormatLocation(session.PickupLocation.Value))}
            """;
    }

    private static void ResetBooking(FleetClientChatSession session, bool keepHistory)
    {
        if (!keepHistory)
            session.History.Clear();

        session.Phase = FleetClientChatPhase.Idle;
        session.RequestedFrom = null;
        session.RequestedTo = null;
        session.SelectedVehicleId = null;
        session.SelectedVehicleLabel = null;
        session.SelectedDailyPrice = null;
        session.LastOffered.Clear();
        session.CustomerName = null;
        session.CustomerEmail = null;
        session.CustomerPhone = null;
        session.PickupLocation = null;
        session.CreatedReservationId = null;
    }

    public string BuildSessionContextForAi(FleetClientChatSession session)
    {
        try
        {
            return JsonSerializer.Serialize(new
            {
                session.Phase,
                session.RequestedFrom,
                session.RequestedTo,
                session.SelectedVehicleLabel,
                session.CustomerName,
                hasEmail = session.CustomerEmail is not null,
                hasPhone = session.CustomerPhone is not null,
                session.PickupLocation,
                offered = session.LastOffered.Select(v => v.Label).Take(6)
            });
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Serijalizacija session konteksta nije uspjela");
            return "{}";
        }
    }
}
