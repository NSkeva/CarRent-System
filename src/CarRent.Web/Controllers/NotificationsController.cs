using CarRent.DAL;
using CarRent.Web.Services;
using CarRent.Web.Services.Notifications;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class NotificationsController(
    CarRentDbContext db,
    FleetNotificationDispatchService dispatchService,
    IOptions<FleetNotificationOptions> notificationOptions) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Obavijesti flote";
        ViewData["NavSection"] = "operativa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Obavijesti");

        var opts = notificationOptions.Value;
        ViewData["EmailDispatchEnabled"] = opts.EmailEnabled && opts.DispatchEnabled && opts.Smtp.IsConfigured;
        ViewData["PushDispatchEnabled"] = opts.PushEnabled && opts.WebPush.IsConfigured;
        ViewData["PendingEmailCount"] = await db.FleetNotificationOutbox
            .CountAsync(n => n.Channel == "Email" && n.SentAt == null);
        ViewData["PendingPushCount"] = await db.FleetNotificationOutbox
            .CountAsync(n => n.Channel == "Push" && n.SentAt == null);
        ViewData["PushSubscriptionCount"] = await db.FleetPushSubscriptions.CountAsync();

        var items = await db.FleetNotificationOutbox
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .ToListAsync();

        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DispatchNow()
    {
        var result = await dispatchService.ProcessPendingAsync();
        var parts = new List<string>();
        if (result.EmailSent > 0) parts.Add($"email: {result.EmailSent}");
        if (result.PushSent > 0) parts.Add($"push: {result.PushSent}");
        var summary = parts.Count > 0 ? string.Join(", ", parts) : "ništa";

        if (result.EmailSkippedReason is not null && result.PushSkippedReason is not null
            && result.EmailPendingCount == 0 && result.PushPendingCount == 0)
            TempData["Error"] = $"Slanje nije aktivno. Email: {result.EmailSkippedReason}. Push: {result.PushSkippedReason}.";
        else if (result.Sent > 0)
            TempData["Success"] = $"Poslano ({summary}). Neuspjelo: {result.Failed}.";
        else if (result.PendingCount == 0)
            TempData["Success"] = "Nema poruka za slanje u outboxu.";
        else
            TempData["Error"] = $"Nije poslano. Email pending: {result.EmailPendingCount}, push pending: {result.PushPendingCount}. Provjeri Gmail / push pretplatu.";

        return RedirectToAction(nameof(Index));
    }
}
