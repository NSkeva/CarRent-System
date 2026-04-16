using CarRent.Web.Repositories;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

public sealed class HomeController(DashboardRepository dashboardRepository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Home";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home");
        return View(dashboardRepository.BuildHomeVm());
    }
}

public sealed class TimelineController(DashboardRepository dashboardRepository) : Controller
{
    public IActionResult Index(string? q, string? vehicleType, string? reservationStatus, string? month)
    {
        var selectedMonth = DateOnly.TryParse(month, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Timeline";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Timeline");
        return View(dashboardRepository.BuildTimelineVm(selectedMonth, q, vehicleType, reservationStatus));
    }
}

public sealed class DailyPlanController(DashboardRepository dashboardRepository) : Controller
{
    public IActionResult Index(string? day)
    {
        var selectedDay = DateOnly.TryParse(day, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["Title"] = "Dnevni plan";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Dnevni plan");
        return View(dashboardRepository.BuildDailyPlanVm(selectedDay));
    }
}

public sealed class FleetController(DashboardRepository dashboardRepository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Vozni park";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozni park");
        return View(dashboardRepository.BuildFleetCards());
    }
}

public sealed class PartnersController(PartnerRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Partneri";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Partneri");
        return View(repository.GetAll());
    }
}

public sealed class BranchOfficeController(BranchOfficeRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Poslovnice";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Poslovnice");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji poslovnice";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Poslovnice", model.Name);
        return View(model);
    }
}

public sealed class VehicleController(VehicleRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Vozila";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozila");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji vozila";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozila", model.RegistrationNumber);
        return View(model);
    }
}

public sealed class CustomerController(CustomerRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Kupci";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Kupci");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji kupca";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Kupci", $"{model.FirstName} {model.LastName}");
        return View(model);
    }
}

public sealed class ReservationController(ReservationRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Rezervacije";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Rezervacije");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji rezervacije";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Rezervacije", $"Rez#{model.Id}");
        return View(model);
    }
}

public sealed class AddonController(AddonRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dodaci";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Dodaci");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji dodatka";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Dodaci", model.Name);
        return View(model);
    }
}

public sealed class ServiceRecordController(ServiceRecordRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Servisi";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Servisi");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji servisa";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Servisi", $"Servis#{model.Id}");
        return View(model);
    }
}

public sealed class EmployeeController(EmployeeRepository repository) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Zaposlenici";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Zaposlenici");
        return View(repository.GetAll());
    }

    public IActionResult Details(int id)
    {
        var model = repository.GetById(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji zaposlenika";
        ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Zaposlenici", $"{model.FirstName} {model.LastName}");
        return View(model);
    }
}
