using CarRent.Model.Entities;
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
        ViewData["NavSection"] = "operativa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home");
        return View(await dashboardRepository.BuildHomeVmAsync());
    }
}

public sealed class TimelineController(
    DashboardRepository dashboardRepository,
    ReservationRepository reservationRepository,
    VehicleRepository vehicleRepository,
    ReservationSchedulingValidator schedulingValidator) : Controller
{
    [Route("raspored/mjesecni")]
    public async Task<IActionResult> Index(string? q, string? vehicleType, string? reservationStatus, string? month)
    {
        var selectedMonth = DateOnly.TryParse(month, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Timeline";
        ViewData["NavSection"] = "operativa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Timeline");
        return View(await dashboardRepository.BuildTimelineVmAsync(selectedMonth, q, vehicleType, reservationStatus));
    }

    [Authorize(Roles = "Admin,Manager")]
    [Route("raspored/mjesecni/modal/rezervacija/nova")]
    public async Task<IActionResult> ModalCreate(int? vehicleId, DateTime? startDate, DateTime? endDate)
    {
        var vm = await BuildCreateFormAsync(vehicleId, startDate, endDate);
        return PartialView("_ReservationModalForm", vm);
    }

    [Authorize]
    [Route("raspored/mjesecni/modal/rezervacija/{id:int}")]
    public async Task<IActionResult> ModalDetails(int id)
    {
        var model = await reservationRepository.GetByIdAsync(id);
        return model is null ? NotFound() : PartialView("_ReservationModalDetails", model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [Route("raspored/mjesecni/modal/rezervacija/{id:int}/uredi")]
    public async Task<IActionResult> ModalEdit(int id)
    {
        var full = await reservationRepository.GetByIdAsync(id);
        return full is null ? NotFound() : PartialView("_ReservationModalForm", EntityMappers.ToForm(full));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    [Route("raspored/mjesecni/modal/rezervacija/nova")]
    public async Task<IActionResult> ModalCreatePost(ReservationFormVm model)
    {
        await schedulingValidator.ValidateAsync(model, ModelState);
        if (!ModelState.IsValid)
            return PartialView("_ReservationModalForm", model);

        var entity = new Reservation();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyReservationLifecycle(entity);
        await reservationRepository.AddAsync(entity);
        return Json(new { success = true, id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    [Route("raspored/mjesecni/modal/rezervacija/{id:int}/uredi")]
    public async Task<IActionResult> ModalEditPost(int id, ReservationFormVm model)
    {
        if (id != model.Id) return BadRequest();
        await schedulingValidator.ValidateAsync(model, ModelState, id);
        if (!ModelState.IsValid)
            return PartialView("_ReservationModalForm", model);

        var entity = await reservationRepository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyReservationLifecycle(entity);
        await reservationRepository.UpdateAsync(entity);
        return Json(new { success = true, id = entity.Id });
    }

    private async Task<ReservationFormVm> BuildCreateFormAsync(int? vehicleId, DateTime? startDate, DateTime? endDate)
    {
        var vm = new ReservationFormVm();
        if (vehicleId is > 0)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(vehicleId.Value);
            if (vehicle is not null)
            {
                vm.VehicleId = vehicle.Id;
                vm.VehicleDisplay = $"{vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber})";
            }
        }

        if (startDate is { } start)
            vm.StartDate = start.Date;
        if (endDate is { } end)
            vm.EndDate = end.Date;
        if (vm.EndDate < vm.StartDate)
            vm.EndDate = vm.StartDate.AddDays(1);

        return vm;
    }
}

public sealed class DailyPlanController(DashboardRepository dashboardRepository) : Controller
{
    [Route("operativa/dnevni-plan")]
    public async Task<IActionResult> Index(string? day)
    {
        var selectedDay = DateOnly.TryParse(day, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Dnevni plan";
        ViewData["NavSection"] = "operativa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Dnevni plan");
        return View(await dashboardRepository.BuildDailyPlanVmAsync(selectedDay));
    }
}

public sealed class FleetController(DashboardRepository dashboardRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Vozni park";
        ViewData["NavSection"] = "operativa";
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
