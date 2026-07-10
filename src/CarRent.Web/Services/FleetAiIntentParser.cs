using System.Text.RegularExpressions;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public static partial class FleetAiIntentParser
{
    [GeneratedRegex(@"(\d+)\s*(?:dana|dan)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RentalDaysRegex();

    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?:\+385|0)\s*\d[\d\s\-]{7,12}\d")]
    private static partial Regex PhoneRegex();

    public static bool IsRentalNeed(string text)
    {
        var lower = text.ToLowerInvariant();
        var wantWords = new[]
        {
            "trebam", "trebao", "trebala", "želim", "zelim", "htio", "htjela", "hoću", "hocu",
            "treba mi", "dao bi", "daj mi", "rezervir", "najm", "iznajm", "voli", "uzm", "book"
        };
        var vehicleWords = new[] { "auto", "vozil", "kombi", "van", "skuter", "motor", "bicikl", "golf", "bmw", "audi", "passat", "model" };

        if (wantWords.Any(w => lower.Contains(w, StringComparison.Ordinal)))
            return true;

        return vehicleWords.Any(v => lower.Contains(v, StringComparison.Ordinal)) &&
               (lower.Contains("vikend", StringComparison.Ordinal) ||
                lower.Contains("danas", StringComparison.Ordinal) ||
                lower.Contains("sutra", StringComparison.Ordinal) ||
                FleetAiDateParser.TryParseRange(text).From is not null);
    }

    public static bool IsAffirmative(string text)
    {
        var lower = text.ToLowerInvariant().Trim();
        var words = new[] { "ok", "oke", "okej", "da", "može", "moze", "super", "ajde", "važi", "vazi", "potvrdi", "slož", "sloz", "u redu", "idemo" };
        return words.Any(w => lower == w || lower.StartsWith(w + " ", StringComparison.Ordinal) || lower.StartsWith(w + ",", StringComparison.Ordinal));
    }

    public static bool ReferencesPreviousChoice(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("taj", StringComparison.Ordinal) ||
               lower.Contains("to ", StringComparison.Ordinal) ||
               lower.Contains("onaj", StringComparison.Ordinal) ||
               lower.Contains("taj model", StringComparison.Ordinal) ||
               lower.Contains("prvi", StringComparison.Ordinal) ||
               lower.Contains("1.", StringComparison.Ordinal) ||
               IsAffirmative(text);
    }

    public static int? TryParseRentalDays(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("tjedan dana", StringComparison.Ordinal) || lower.Contains("jedan tjedan", StringComparison.Ordinal))
            return 7;
        if (lower.Contains("vikend", StringComparison.Ordinal) && !lower.Contains("ovaj vikend", StringComparison.Ordinal))
            return 2;

        var match = RentalDaysRegex().Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var days) && days is > 0 and <= 60)
            return days;

        return null;
    }

    public static void TryExtractContact(string text, FleetClientChatSession session)
    {
        var email = EmailRegex().Match(text);
        if (email.Success)
            session.CustomerEmail = email.Value;

        var phone = PhoneRegex().Match(text);
        if (phone.Success)
            session.CustomerPhone = Regex.Replace(phone.Value, @"\s+", "");

        var lower = text.ToLowerInvariant();
        if (lower.Contains("zovem se", StringComparison.Ordinal) || lower.Contains("ime je", StringComparison.Ordinal) || lower.Contains("ime mi je", StringComparison.Ordinal))
        {
            var idx = lower.IndexOf("zovem se", StringComparison.Ordinal);
            if (idx < 0) idx = lower.IndexOf("ime je", StringComparison.Ordinal);
            if (idx < 0) idx = lower.IndexOf("ime mi je", StringComparison.Ordinal);
            var namePart = text[(idx + (lower.Contains("ime mi je") ? 9 : lower.Contains("ime je") ? 6 : 8))..].Trim().TrimEnd('.', ',');
            if (namePart.Length >= 2)
                session.CustomerName = CapitalizeName(namePart);
        }
        else if (session.CustomerName is null && session.Phase == FleetClientChatPhase.CollectingContact &&
                 !email.Success && !phone.Success && text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length is >= 2 and <= 4)
        {
            session.CustomerName = CapitalizeName(text.Trim());
        }

        var location = TryParsePickupLocation(text);
        if (location.HasValue)
            session.PickupLocation = location;
    }

    public static LocationType? TryParsePickupLocation(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("aerodrom", StringComparison.Ordinal) || lower.Contains("zračn", StringComparison.Ordinal) || lower.Contains("airport", StringComparison.Ordinal))
            return LocationType.Airport;
        if (lower.Contains("centar", StringComparison.Ordinal) || lower.Contains("downtown", StringComparison.Ordinal))
            return LocationType.Downtown;
        if (lower.Contains("hotel", StringComparison.Ordinal))
            return LocationType.HotelPartner;
        if (lower.Contains("poslovnic", StringComparison.Ordinal) || lower.Contains("ured", StringComparison.Ordinal))
            return LocationType.MainOffice;
        return null;
    }

    public static string? NextMissingContactField(FleetClientChatSession session) => session switch
    {
        { CustomerName: null } => "Kako se zovete?",
        { CustomerEmail: null } => "Na koji email da pošaljemo potvrdu?",
        { CustomerPhone: null } => "Molim vas i broj mobitela.",
        { PickupLocation: null } => "Gdje želite preuzeti vozilo? (npr. aerodrom, centar ili glavna poslovnica)",
        _ => null
    };

    public static string FormatLocation(LocationType location) => location switch
    {
        LocationType.Airport => "Aerodrom",
        LocationType.Downtown => "Centar",
        LocationType.HotelPartner => "Hotel partner",
        LocationType.MainOffice => "Glavna poslovnica",
        LocationType.RemoteDropoff => "Dogovorena lokacija",
        _ => location.ToString()
    };

    private static string CapitalizeName(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(p =>
            p.Length == 0 ? p : char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }
}
