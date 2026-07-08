using CarRent.DAL;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CarRent.Web.Services;

public sealed class AiClientChatService(
    CarRentDbContext db,
    IConfiguration config,
    IHttpClientFactory httpClientFactory,
    ILogger<AiClientChatService> logger)
{
    public async Task<string> GetReplyAsync(string userMessage, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Pošaljite pitanje — npr. „Ima li slobodnih vozila ovaj vikend?”";

        var apiKey = config["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var ai = await TryOpenAiAsync(userMessage, apiKey, ct);
            if (!string.IsNullOrWhiteSpace(ai))
                return ai;
        }

        return await GetRuleBasedReplyAsync(userMessage, ct);
    }

    private async Task<string?> TryOpenAiAsync(string userMessage, string apiKey, CancellationToken ct)
    {
        try
        {
            var context = await BuildFleetContextAsync(ct);
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var body = new
            {
                model = config["OpenAI:Model"] ?? "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = """
                        Ti si korisnički asistent rent-a car CarRent. Odgovaraj na hrvatskom, kratko i jasno.
                        Koristi kontekst flote ispod. Ako korisnik želi rezervaciju, pitaj za datume i tip vozila.
                        """ + "\n\n" + context },
                    new { role = "user", content = userMessage }
                },
                max_tokens = 400
            };

            using var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenAI odgovor {Status}", response.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenAI poziv nije uspio");
            return null;
        }
    }

    private async Task<string> BuildFleetContextAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var vehicles = await db.Vehicles.AsNoTracking()
            .Where(v => v.IsActive)
            .Select(v => new { v.Brand, v.Model, v.RegistrationNumber, v.DailyPrice, v.Type })
            .Take(15)
            .ToListAsync(ct);

        var busyIds = await db.Reservations.AsNoTracking()
            .Where(r => r.StartDate.Date <= today && r.EndDate.Date >= today &&
                        (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Draft))
            .Select(r => r.VehicleId)
            .Distinct()
            .ToListAsync(ct);

        var freeCount = vehicles.Count - busyIds.Count;
        var lines = vehicles.Select(v =>
            $"- {v.Brand} {v.Model} ({v.RegistrationNumber}), {v.Type}, {v.DailyPrice:N0} EUR/dan");

        return $"""
            Aktivnih vozila: {vehicles.Count}
            Procijenjeno slobodnih danas: {Math.Max(0, freeCount)}
            Vozila:
            {string.Join("\n", lines)}
            """;
    }

    private async Task<string> GetRuleBasedReplyAsync(string userMessage, CancellationToken ct)
    {
        var msg = userMessage.ToLowerInvariant();

        if (msg.Contains("slobod") || msg.Contains("dostup") || msg.Contains("ima li"))
        {
            var today = DateTime.UtcNow.Date;
            var active = await db.Vehicles.AsNoTracking().CountAsync(v => v.IsActive, ct);
            var busy = await db.Reservations.AsNoTracking()
                .Where(r => r.StartDate.Date <= today && r.EndDate.Date >= today &&
                            (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active))
                .Select(r => r.VehicleId)
                .Distinct()
                .CountAsync(ct);
            var free = Math.Max(0, active - busy);
            return $"Trenutno imamo oko {free} slobodnih vozila od {active} aktivnih u floti. " +
                   "Za točan termin pošaljite datume (npr. 15.–20.6.) ili pogledajte Vozni park na webu.";
        }

        if (msg.Contains("rezerv") || msg.Contains("najam") || msg.Contains("iznajm"))
            return "Za rezervaciju trebam datume i tip vozila. Možete nastaviti: „Želim SUV od 10. do 15. lipnja” " +
                   "ili se prijavite kao Manager/Admin i otvorite Rezervacije → Nova rezervacija.";

        if (msg.Contains("cijen") || msg.Contains("koliko košta") || msg.Contains("eur"))
        {
            var avg = await db.Vehicles.AsNoTracking().Where(v => v.IsActive).AverageAsync(v => (double?)v.DailyPrice, ct);
            return avg is null
                ? "Nemam podataka o cijenama u bazi."
                : $"Prosječna dnevna cijena aktivnih vozila je oko {avg:N0} EUR. Točna cijena ovisi o modelu.";
        }

        if (msg.Contains("pozdrav") || msg.Contains("bok") || msg.Contains("hello"))
            return "Bok! Ja sam CarRent asistent. Pitajte me o slobodnim vozilima, cijenama ili rezervacijama.";

        return "Mogu pomoći oko dostupnosti vozila, cijena i rezervacija. Primjer: „Ima li slobodnih automobila ovaj vikend?”";
    }
}
