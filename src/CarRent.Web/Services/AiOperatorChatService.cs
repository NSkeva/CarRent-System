using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Services;

public sealed class AiOperatorChatService(
    CarRentDbContext db,
    FleetAiAvailabilityService availabilityService,
    IConfiguration config,
    IHttpClientFactory httpClientFactory,
    ILogger<AiOperatorChatService> logger)
{
    public async Task<string> GetReplyAsync(string userMessage, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Pitajte o današnjem planu, slobodnim vozilima, nacrtima ili rezervacijama — npr. „Što je danas na rasporedu?”";

        var context = await BuildOperatorContextAsync(ct);
        var ai = await FleetAiOpenAiHelper.TryCompleteAsync(
            httpClientFactory,
            config,
            logger,
            """
            Ti si interni operativni asistent rent-a car CarRent za Admin/Manager tim.
            Odgovaraj na hrvatskom, kratko i konkretno. Koristi kontekst ispod iz baze.
            Za akcije daj korisne linkove: Timeline (/raspored), Dnevni plan (/dnevni-plan),
            Nova rezervacija (/Reservation/Create), Vozni park (/vozni-park).
            Ne izmišljaj podatke — ako nešto nije u kontekstu, reci da provjere u aplikaciji.
            """ + "\n\n" + context,
            userMessage,
            ct);

        if (!string.IsNullOrWhiteSpace(ai))
            return ai;

        return await GetRuleBasedReplyAsync(userMessage, ct);
    }

    private async Task<string> BuildOperatorContextAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var todayOnly = DateOnly.FromDateTime(today);

        var departures = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.StartDate.Date == today &&
                        (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active))
            .OrderBy(r => r.StartDate)
            .Take(12)
            .ToListAsync(ct);

        var returns = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.EndDate.Date == today && r.Status == ReservationStatus.Active)
            .OrderBy(r => r.EndDate)
            .Take(12)
            .ToListAsync(ct);

        var activeCount = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.Status == ReservationStatus.Active, ct);

        var draftCount = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.Status == ReservationStatus.Draft, ct);

        var inactiveVehicles = await db.Vehicles.AsNoTracking()
            .Where(v => !v.IsActive)
            .Select(v => $"{v.Brand} {v.Model} ({v.RegistrationNumber})")
            .Take(8)
            .ToListAsync(ct);

        var availability = await availabilityService.GetAvailabilityAsync(todayOnly, todayOnly, ct: ct);

        static string FormatReservation(Reservation r, string action)
        {
            var customer = r.Customer is null ? "kupac" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            var vehicle = r.Vehicle?.RegistrationNumber ?? $"vozilo #{r.VehicleId}";
            return $"- #{r.Id} {customer}, {vehicle} — {action} {r.PickupLocation}→{r.DropoffLocation}";
        }

        return $"""
            Datum: {today:dd.MM.yyyy.}
            Aktivnih najmova: {activeCount}
            Nacrta (Draft): {draftCount}
            Slobodnih vozila danas: {availability.FreeVehicles.Count} / {availability.TotalActive}

            Odlasci danas ({departures.Count}):
            {(departures.Count == 0 ? "- nema" : string.Join("\n", departures.Select(r => FormatReservation(r, "preuzimanje"))))}

            Povrati danas ({returns.Count}):
            {(returns.Count == 0 ? "- nema" : string.Join("\n", returns.Select(r => FormatReservation(r, "povrat"))))}

            Neaktivna vozila (servis/hold, {inactiveVehicles.Count}):
            {(inactiveVehicles.Count == 0 ? "- sva aktivna" : string.Join("\n", inactiveVehicles.Select(v => "- " + v)))}
            """;
    }

    private async Task<string> GetRuleBasedReplyAsync(string userMessage, CancellationToken ct)
    {
        var msg = userMessage.ToLowerInvariant();
        var today = DateTime.UtcNow.Date;
        var todayOnly = DateOnly.FromDateTime(today);

        if (msg.Contains("danas") && (msg.Contains("plan") || msg.Contains("raspored") || msg.Contains("što") || msg.Contains("sta")))
            return await BuildTodaySummaryAsync(today, ct);

        if (msg.Contains("odlaz") || msg.Contains("preuzim") || msg.Contains("polaz"))
            return await BuildDeparturesReplyAsync(today, ct);

        if (msg.Contains("povrat") || msg.Contains("vrać") || msg.Contains("vrac"))
            return await BuildReturnsReplyAsync(today, ct);

        if (msg.Contains("nacrt") || msg.Contains("draft"))
            return await BuildDraftsReplyAsync(ct);

        if (msg.Contains("aktivn") || msg.Contains("u najmu"))
        {
            var count = await db.Reservations.AsNoTracking()
                .CountAsync(r => r.Status == ReservationStatus.Active, ct);
            return $"Trenutno je {count} aktivnih najmova. Detalji: Dnevni plan (/dnevni-plan) ili Timeline (/raspored).";
        }

        if (msg.Contains("servis") || msg.Contains("neaktivn"))
        {
            var inactive = await db.Vehicles.AsNoTracking()
                .Where(v => !v.IsActive)
                .Select(v => $"{v.Brand} {v.Model} ({v.RegistrationNumber})")
                .Take(10)
                .ToListAsync(ct);
            return inactive.Count == 0
                ? "Sva vozila su aktivna u floti."
                : $"Neaktivna vozila ({inactive.Count}):\n" + string.Join("\n", inactive.Select(v => "- " + v)) +
                  "\n\nProvjeri Servisi u Podacima ili Vozni park.";
        }

        if (msg.Contains("slobod") || msg.Contains("dostup") || msg.Contains("ima li"))
        {
            var (from, to) = FleetAiDateParser.TryParseRange(userMessage);
            from ??= todayOnly;
            to ??= from;
            var typeFilter = FleetAiDateParser.TryParseVehicleType(userMessage);
            var result = await availabilityService.GetAvailabilityAsync(from.Value, to.Value, typeFilter, ct);
            return FleetAiAvailabilityService.FormatClientAvailabilityReply(result, typeFilter) +
                   "\n\nZa raspored: Timeline (/raspored).";
        }

        if (msg.Contains("nova rezerv") || msg.Contains("napravi rezerv") || msg.Contains("kako rezerv"))
            return "Nova rezervacija: Podaci → Rezervacije → Nova (/Reservation/Create). " +
                   "Za brzi pregled zauzetosti koristi Timeline (/raspored).";

        if (msg.Contains("timeline") || msg.Contains("konflikt") || msg.Contains("raspored"))
            return "Vizualni raspored i konflikti: Timeline (/raspored). Dnevni plan za današnje zadatke: /dnevni-plan.";

        if (msg.Contains("cijen") || msg.Contains("koliko"))
        {
            var avg = await db.Vehicles.AsNoTracking().Where(v => v.IsActive).AverageAsync(v => (double?)v.DailyPrice, ct);
            return avg is null
                ? "Nema aktivnih vozila s cijenom."
                : $"Prosječna dnevna cijena aktivnih vozila: {avg:N0} EUR. Detalji po modelu: Vozni park (/vozni-park).";
        }

        if (msg.Contains("pozdrav") || msg.Contains("bok") || msg.Contains("hello"))
            return "Bok! Ja sam operativni AI asistent. Pitaj me što je danas na planu, slobodna vozila ili nacrti rezervacija.";

        return "Mogu pomoći s današnjim odlascima/povratima, slobodnim vozilima, nacrtima i navigacijom. " +
               "Primjer: „Što je danas na rasporedu?” ili „Ima li slobodnog kombija ovaj vikend?”";
    }

    private async Task<string> BuildTodaySummaryAsync(DateTime today, CancellationToken ct)
    {
        var departures = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.StartDate.Date == today &&
                             (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active), ct);
        var returns = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.EndDate.Date == today && r.Status == ReservationStatus.Active, ct);
        var active = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.Status == ReservationStatus.Active, ct);
        var drafts = await db.Reservations.AsNoTracking()
            .CountAsync(r => r.Status == ReservationStatus.Draft, ct);

        return $"Danas ({today:dd.MM.yyyy.}): {departures} odlazaka, {returns} povrata, {active} aktivnih najmova, {drafts} nacrta. " +
               "Detalji: Dnevni plan (/dnevni-plan) ili pitaj „tko preuzima danas” / „tko vraća danas”.";
    }

    private async Task<string> BuildDeparturesReplyAsync(DateTime today, CancellationToken ct)
    {
        var items = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.StartDate.Date == today &&
                        (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active))
            .OrderBy(r => r.StartDate)
            .Take(10)
            .ToListAsync(ct);

        if (items.Count == 0)
            return "Danas nema planiranih preuzimanja vozila.";

        var lines = items.Select(r =>
        {
            var customer = r.Customer is null ? "kupac" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            return $"- #{r.Id} {customer}, {r.Vehicle?.RegistrationNumber ?? "?"} — {r.PickupLocation}";
        });

        return $"Preuzimanja danas ({items.Count}):\n{string.Join("\n", lines)}";
    }

    private async Task<string> BuildReturnsReplyAsync(DateTime today, CancellationToken ct)
    {
        var items = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Where(r => r.EndDate.Date == today && r.Status == ReservationStatus.Active)
            .OrderBy(r => r.EndDate)
            .Take(10)
            .ToListAsync(ct);

        if (items.Count == 0)
            return "Danas nema planiranih povrata vozila.";

        var lines = items.Select(r =>
        {
            var customer = r.Customer is null ? "kupac" : $"{r.Customer.FirstName} {r.Customer.LastName}";
            return $"- #{r.Id} {customer}, {r.Vehicle?.RegistrationNumber ?? "?"} — {r.DropoffLocation}";
        });

        return $"Povrati danas ({items.Count}):\n{string.Join("\n", lines)}";
    }

    private async Task<string> BuildDraftsReplyAsync(CancellationToken ct)
    {
        var drafts = await db.Reservations.AsNoTracking()
            .Include(r => r.Vehicle)
            .Where(r => r.Status == ReservationStatus.Draft)
            .OrderByDescending(r => r.CreatedAt)
            .Take(8)
            .ToListAsync(ct);

        if (drafts.Count == 0)
            return "Nema nacrta rezervacija. Nova: /Reservation/Create";

        var lines = drafts.Select(r =>
            $"- #{r.Id} {r.Vehicle?.RegistrationNumber ?? "vozilo"}, {r.StartDate:dd.MM.}–{r.EndDate:dd.MM.}");

        return $"Nacrti ({drafts.Count} prikazano):\n{string.Join("\n", lines)}\n\nPotvrdi ili uredi u Rezervacijama.";
    }
}
