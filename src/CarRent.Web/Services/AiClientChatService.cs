namespace CarRent.Web.Services;

public sealed class AiClientChatService(
    FleetAiAvailabilityService availability,
    FleetClientChatConversation conversation,
    IConfiguration config,
    IHttpClientFactory httpClientFactory,
    ILogger<AiClientChatService> logger)
{
    public async Task<string> GetReplyAsync(
        string userMessage,
        FleetClientChatSession session,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Recite mi što vam treba — npr. „Trebam auto ovaj vikend”.";

        var deterministic = await conversation.ProcessAsync(session, userMessage, ct);
        var fleetContext = await BuildFleetSnapshotAsync(ct);
        var sessionContext = conversation.BuildSessionContextForAi(session);

        var ai = await FleetAiOpenAiHelper.TryConversationAsync(
            httpClientFactory,
            config,
            logger,
            """
            Ti si pametni klijentski asistent rent-a car CarRent. Odgovaraj na hrvatskom, prirodno i kratko.
            Vodiš razgovor u više koraka prema rezervaciji: datumi → izbor vozila → kontakt podaci → potvrda.
            Zaključuj iz konteksta (npr. "taj model", "7 dana", "ovaj vikend") — ne traži od korisnika da sve napiše odjednom.
            Koristi stanje razgovora i flotu ispod. Pitaj samo za podatke koji još nedostaju.
            Ne izmišljaj vozila koja nisu u ponudi. Ne spominji interne admin stranice.
            Odgovor sustava (činjenice koje moraš poštovati): 
            """ + deterministic + "\n\nStanje razgovora:\n" + sessionContext + "\n\nFlota:\n" + fleetContext,
            session.History,
            userMessage,
            ct);

        var reply = !string.IsNullOrWhiteSpace(ai) ? ai : deterministic;

        session.AddTurn("user", userMessage);
        session.AddTurn("assistant", reply);
        return reply;
    }

    private async Task<string> BuildFleetSnapshotAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var snapshot = await availability.GetAvailabilityAsync(today, today.AddDays(7), ct: ct);

        var lines = snapshot.FreeVehicles
            .Take(15)
            .Select(v =>
                $"- id={v.Id} {v.Brand} {v.Model} ({v.RegistrationNumber}), {FleetAiAvailabilityService.FormatVehicleType(v.Type)}, {v.DailyPrice:N0} EUR/dan")
            .ToList();

        return $"""
            Danas: {today:dd.MM.yyyy.}
            Aktivnih vozila: {snapshot.TotalActive}
            Slobodnih u sljedećih 7 dana (ukupno u periodu): {snapshot.FreeVehicles.Count}
            Primjeri slobodnih vozila:
            {(lines.Count == 0 ? "- provjeri konkretan termin" : string.Join("\n", lines))}
            """;
    }
}
