using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Services;

public static class NavContext
{
    public const string OperativaSection = "operativa";

    public static string? GetReturnUrl(HttpRequest request)
        => request.Query["returnUrl"].FirstOrDefault();

    public static string? GetReturnUrlFromForm(IFormCollection form)
        => form["returnUrl"].FirstOrDefault();

    public static bool IsOperativaContext(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return false;

        return returnUrl.Contains("vozni-park", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Contains("/Fleet", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Contains("/Timeline", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Contains("/DailyPlan", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Contains("operativa/dnevni-plan", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Equals("/", StringComparison.OrdinalIgnoreCase)
            || returnUrl.Contains("/Home", StringComparison.OrdinalIgnoreCase);
    }

    public static void ApplyVehicle(
        Controller controller,
        string? returnUrl = null,
        string? registrationNumber = null,
        string? editLabel = null)
    {
        returnUrl ??= GetReturnUrl(controller.Request);
        if (!string.IsNullOrWhiteSpace(returnUrl))
            controller.ViewData["ReturnUrl"] = returnUrl;

        if (!IsOperativaContext(returnUrl)) return;

        controller.ViewData["NavSection"] = OperativaSection;
        var leaf = editLabel ?? registrationNumber;
        if (leaf is not null)
            controller.ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozni park", leaf);
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
