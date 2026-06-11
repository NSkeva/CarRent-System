using CarRent.Web.Repositories;
using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

public sealed class HomeController(DashboardRepository dashboardRepository) : Controller
{
    [Route("/")]
    [Route("pocetna")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Home";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home");
        return View(await dashboardRepository.BuildHomeVmAsync());
    }
}

public sealed class TimelineController(DashboardRepository dashboardRepository) : Controller
{
    [Route("raspored/mjesecni")]
    public async Task<IActionResult> Index(string? q, string? vehicleType, string? reservationStatus, string? month)
    {
        var selectedMonth = DateOnly.TryParse(month, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Timeline";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Timeline");
        return View(await dashboardRepository.BuildTimelineVmAsync(selectedMonth, q, vehicleType, reservationStatus));
    }
}

public sealed class DailyPlanController(DashboardRepository dashboardRepository) : Controller
{
    [Route("operativa/dnevni-plan")]
    public async Task<IActionResult> Index(string? day)
    {
        var selectedDay = DateOnly.TryParse(day, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Dnevni plan";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Dnevni plan");
        return View(await dashboardRepository.BuildDailyPlanVmAsync(selectedDay));
    }
}

public sealed class FleetController(DashboardRepository dashboardRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Vozni park";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozni park");
        return View(await dashboardRepository.BuildFleetCardsAsync());
    }
}

[Route("partneri")]
public sealed class PartnersController(PartnerRepository repository) : Controller
{
    [HttpGet(""), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Partneri";
        return View(await repository.GetAllAsync());
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Route("novi"), Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novi partner";
        return View(new PartnerFormVm());
    }

    [HttpPost, Route("novi"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(PartnerFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        await repository.AddAsync(EntityMappers.ToEntity(model));
        return RedirectToAction(nameof(Index));
    }

    [Route("uredi/{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi partnera";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, Route("uredi/{id:int}"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, PartnerFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Route("obrisi/{id:int}"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
