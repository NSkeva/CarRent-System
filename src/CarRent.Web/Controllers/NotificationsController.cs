using CarRent.DAL;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class NotificationsController(CarRentDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Pripremljene obavijesti";
        ViewData["NavSection"] = "operativa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Obavijesti");

        var items = await db.FleetNotificationOutbox
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .ToListAsync();

        return View(items);
    }
}
