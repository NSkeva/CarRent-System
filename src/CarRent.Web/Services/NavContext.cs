using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Services;

public static class NavContext
{
    public const string OperativaSection = "operativa";

    private static readonly (Func<string, bool> Match, string Label, string Crumb)[] OperativaSources =
    [
        (url => url.Contains("vozni-park", StringComparison.OrdinalIgnoreCase) || url.Contains("/Fleet", StringComparison.OrdinalIgnoreCase), "Vozni park", "Vozni park"),
        (url => url.Contains("raspored", StringComparison.OrdinalIgnoreCase) || url.Contains("/Timeline", StringComparison.OrdinalIgnoreCase), "Timeline", "Timeline"),
        (url => url.Contains("dnevni-plan", StringComparison.OrdinalIgnoreCase) || url.Contains("/DailyPlan", StringComparison.OrdinalIgnoreCase), "Dnevni plan", "Dnevni plan"),
        (url => url.Contains("/Notifications", StringComparison.OrdinalIgnoreCase) || url.Contains("obavijesti", StringComparison.OrdinalIgnoreCase), "Obavijesti", "Obavijesti"),
        (url => url.Contains("/OperatorAi", StringComparison.OrdinalIgnoreCase) ||
                 url.Contains("ai-asistent", StringComparison.OrdinalIgnoreCase) ||
                 url.Contains("/ClientChat", StringComparison.OrdinalIgnoreCase), "AI asistent", "AI asistent"),
        (url => url is "/" or "" || url.Contains("/pocetna", StringComparison.OrdinalIgnoreCase) || url.Contains("/Home", StringComparison.OrdinalIgnoreCase), "Početna", "Home"),
    ];

    public static string CurrentPageUrl(HttpRequest request)
    {
        var path = request.Path.Value ?? "/";
        var query = request.QueryString.Value ?? "";
        return path + query;
    }

    public static string? GetReturnUrl(HttpRequest request)
        => request.Query["returnUrl"].FirstOrDefault();

    public static string? GetReturnUrlFromForm(IFormCollection form)
        => form["returnUrl"].FirstOrDefault();

    public static bool IsOperativaContext(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return false;
        return OperativaSources.Any(source => source.Match(returnUrl));
    }

    public static string? GetOperativaLabel(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        foreach (var source in OperativaSources)
            if (source.Match(returnUrl))
                return source.Label;
        return null;
    }

    private static string? GetOperativaCrumb(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        foreach (var source in OperativaSources)
            if (source.Match(returnUrl))
                return source.Crumb;
        return null;
    }

    public static void Apply(
        Controller controller,
        string? returnUrl,
        string podaciArea,
        string? leaf = null)
    {
        returnUrl ??= GetReturnUrl(controller.Request);
        if (!string.IsNullOrWhiteSpace(returnUrl))
            controller.ViewData["ReturnUrl"] = returnUrl;

        if (IsOperativaContext(returnUrl))
        {
            controller.ViewData["NavSection"] = OperativaSection;
            controller.ViewData["BackLabel"] = GetOperativaLabel(returnUrl);
            var section = GetOperativaCrumb(returnUrl) ?? "Operativa";
            controller.ViewData["Breadcrumbs"] = leaf is null
                ? BreadcrumbHelper.Build("Home", section)
                : BreadcrumbHelper.Build("Home", section, leaf);
            return;
        }

        if (leaf is not null)
            controller.ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", podaciArea, leaf);
    }

    public static void ApplyVehicle(
        Controller controller,
        string? returnUrl = null,
        string? registrationNumber = null,
        string? editLabel = null)
    {
        var leaf = editLabel ?? registrationNumber;
        Apply(controller, returnUrl, "Vozila", leaf);
    }

    public static IActionResult RedirectAfterVehicleSave(Controller controller, int vehicleId, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
            return new RedirectResult(returnUrl);

        if (IsOperativaContext(returnUrl))
            return new RedirectToActionResult("Details", "Vehicle", new { id = vehicleId, returnUrl });

        return new RedirectToActionResult("Index", "Vehicle", null);
    }

    public static IActionResult RedirectToReturnUrl(Controller controller, string? returnUrl, IActionResult fallback)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
            return new RedirectResult(returnUrl);

        return fallback;
    }
}
