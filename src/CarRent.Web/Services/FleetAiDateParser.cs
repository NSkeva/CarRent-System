using System.Globalization;
using System.Text.RegularExpressions;
using CarRent.Model.Enums;

namespace CarRent.Web.Services;

public static partial class FleetAiDateParser
{
    private static readonly CultureInfo Hr = CultureInfo.GetCultureInfo("hr-HR");

    [GeneratedRegex(@"(\d{1,2})\s*\.\s*(\d{1,2})\s*\.(?:\s*(\d{4}))?\s*(?:[-–—]|do)\s*(\d{1,2})\s*\.\s*(\d{1,2})\s*\.(?:\s*(\d{4}))?", RegexOptions.IgnoreCase)]
    private static partial Regex DateRangeRegex();

    [GeneratedRegex(@"(\d{1,2})\s*\.\s*(\d{1,2})\s*\.(?:\s*(\d{4}))?", RegexOptions.IgnoreCase)]
    private static partial Regex SingleDateRegex();

    public static (DateOnly? From, DateOnly? To) TryParseRange(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, null);

        var lower = text.ToLowerInvariant();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        if (lower.Contains("danas", StringComparison.Ordinal))
            return (today, today);

        if (lower.Contains("sutra", StringComparison.Ordinal))
        {
            var tomorrow = today.AddDays(1);
            return (tomorrow, tomorrow);
        }

        if (lower.Contains("vikend", StringComparison.Ordinal))
            return GetWeekendRange(today);

        var rangeMatch = DateRangeRegex().Match(text);
        if (rangeMatch.Success)
        {
            var fromYear = ParseYear(rangeMatch.Groups[3].Value, today.Year);
            var toYear = ParseYear(rangeMatch.Groups[6].Value, fromYear);
            if (TryCreateDate(rangeMatch.Groups[1].Value, rangeMatch.Groups[2].Value, fromYear, out var from) &&
                TryCreateDate(rangeMatch.Groups[4].Value, rangeMatch.Groups[5].Value, toYear, out var to))
                return (from, to < from ? from : to);
        }

        var singleMatch = SingleDateRegex().Match(text);
        if (singleMatch.Success)
        {
            var year = ParseYear(singleMatch.Groups[3].Value, today.Year);
            if (TryCreateDate(singleMatch.Groups[1].Value, singleMatch.Groups[2].Value, year, out var date))
                return (date, date);
        }

        return (null, null);
    }

    public static VehicleType? TryParseVehicleType(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("kombi", StringComparison.Ordinal) || lower.Contains("van", StringComparison.Ordinal))
            return VehicleType.Van;
        if (lower.Contains("skuter", StringComparison.Ordinal) || lower.Contains("scooter", StringComparison.Ordinal))
            return VehicleType.Scooter;
        if (lower.Contains("motor", StringComparison.Ordinal))
            return VehicleType.Motorcycle;
        if (lower.Contains("bicikl", StringComparison.Ordinal) || lower.Contains("bike", StringComparison.Ordinal))
            return VehicleType.Bicycle;
        if (lower.Contains("automobil", StringComparison.Ordinal) || lower.Contains("limuzin", StringComparison.Ordinal))
            return VehicleType.Car;

        return null;
    }

    public static string FormatRange(DateOnly from, DateOnly to)
        => from == to
            ? from.ToString("d.M.yyyy.", Hr)
            : $"{from.ToString("d.M.", Hr)}–{to.ToString("d.M.yyyy.", Hr)}";

    private static (DateOnly From, DateOnly To) GetWeekendRange(DateOnly today)
    {
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        var saturday = today.AddDays(daysUntilSaturday);
        return (saturday, saturday.AddDays(1));
    }

    private static int ParseYear(string value, int fallback)
        => int.TryParse(value, out var year) ? year : fallback;

    private static bool TryCreateDate(string dayText, string monthText, int year, out DateOnly date)
    {
        date = default;
        if (!int.TryParse(dayText, out var day) || !int.TryParse(monthText, out var month))
            return false;

        try
        {
            date = new DateOnly(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
