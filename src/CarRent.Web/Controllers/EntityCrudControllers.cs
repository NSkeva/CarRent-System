using CarRent.Model.Entities;
using CarRent.Web.Repositories;
using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

public sealed class BranchOfficeController(BranchOfficeRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Nova poslovnica";
        return View(new BranchOfficeFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchOfficeFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new BranchOffice();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi poslovnicu";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, BranchOfficeFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await repository.DeleteAsync(id))
            TempData["Error"] = "Poslovnicu nije moguće obrisati dok ima povezanih vozila.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji poslovnice";
        return View(model);
    }
}

public sealed class VehicleController(VehicleRepository repository, BranchOfficeRepository branches) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Novo vozilo";
        return View(new VehicleFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormVm form)
    {
        if (!ModelState.IsValid) return View(form);
        var entity = new Vehicle();
        EntityMappers.Apply(form, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var branch = await branches.GetTrackedAsync(entity.BranchOfficeId);
        ViewData["Title"] = "Uredi vozilo";
        return View(EntityMappers.ToForm(entity, branch?.Name));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, VehicleFormVm form)
    {
        if (id != form.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", form);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(form, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji vozila";
        return View(model);
    }

    [Route("reg/{registration}")]
    public async Task<IActionResult> ByRegistration(string registration)
    {
        var model = await repository.GetByRegistrationAsync(registration);
        if (model is null) return NotFound();
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
}

public sealed class CustomerController(CustomerRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Novi kupac";
        return View(new CustomerFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Customer();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi kupca";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, CustomerFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji kupca";
        return View(model);
    }
}

public sealed class ReservationController(ReservationRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Nova rezervacija";
        return View(new ReservationFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormVm model)
    {
        ValidateDates(model);
        if (!ModelState.IsValid) return View(model);
        var entity = new Reservation();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var full = await repository.GetByIdAsync(id);
        if (full is null) return NotFound();
        ViewData["Title"] = "Uredi rezervaciju";
        return View(EntityMappers.ToForm(full));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, ReservationFormVm model)
    {
        if (id != model.Id) return BadRequest();
        ValidateDates(model);
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [Route("rezervacije/pregled/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji rezervacije";
        return View(model);
    }

    private void ValidateDates(ReservationFormVm model)
    {
        if (model.EndDate <= model.StartDate)
            ModelState.AddModelError(nameof(model.EndDate), "Datum završetka mora biti nakon početka.");
    }
}

public sealed class AddonController(AddonRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Novi dodatak";
        return View(new AddonFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddonFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Addon();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi dodatak";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, AddonFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji dodatka";
        return View(model);
    }
}

public sealed class ServiceRecordController(ServiceRecordRepository repository, VehicleRepository vehicles) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Novi servis";
        return View(new ServiceRecordFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRecordFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new ServiceRecord();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var vehicle = await vehicles.GetByIdAsync(entity.VehicleId);
        var label = vehicle is null ? string.Empty : $"{vehicle.RegistrationNumber} - {vehicle.Brand} {vehicle.Model}";
        ViewData["Title"] = "Uredi servis";
        return View(EntityMappers.ToForm(entity, label));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, ServiceRecordFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji servisa";
        return View(model);
    }
}

public sealed class EmployeeController(EmployeeRepository repository, BranchOfficeRepository branches) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    public IActionResult Create()
    {
        ViewData["Title"] = "Novi zaposlenik";
        return View(new EmployeeFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Employee();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var branch = await branches.GetTrackedAsync(entity.BranchOfficeId);
        ViewData["Title"] = "Uredi zaposlenika";
        return View(EntityMappers.ToForm(entity, branch?.Name));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, EmployeeFormVm model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", model);
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        await repository.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji zaposlenika";
        return View(model);
    }
}
