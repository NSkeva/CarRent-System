using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Repositories;
using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Controllers;

public sealed class BranchOfficeController(BranchOfficeRepository repository) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Nova poslovnica";
        return View(new BranchOfficeFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(BranchOfficeFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new BranchOffice();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi poslovnicu";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
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

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
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

public sealed class VehicleController(
    VehicleRepository repository,
    BranchOfficeRepository branches,
    CarRentDbContext db,
    IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novo vozilo";
        return View(new VehicleFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(VehicleFormVm form)
    {
        if (!ModelState.IsValid) return View(form);
        var entity = new Vehicle();
        EntityMappers.Apply(form, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var branch = await branches.GetTrackedAsync(entity.BranchOfficeId);
        ViewData["Title"] = "Uredi vozilo";
        ViewData["VehicleId"] = id;
        NavContext.ApplyVehicle(this, returnUrl, entity.RegistrationNumber, "Uredi vozilo");
        ViewData["MainImageUrl"] = VehicleImageHelper.GetDisplayUrl(entity);
        ViewData["HasCustomMainImage"] = VehicleImageHelper.HasCustomImage(entity);
        return View(EntityMappers.ToForm(entity, branch?.Name));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditPost(int id, VehicleFormVm form)
    {
        if (id != form.Id) return BadRequest();
        var returnUrl = NavContext.GetReturnUrlFromForm(Request.Form);
        if (!ModelState.IsValid)
        {
            NavContext.ApplyVehicle(this, returnUrl, editLabel: "Uredi vozilo");
            return View("Edit", form);
        }
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var previousMileage = entity.MileageKm;
        EntityMappers.Apply(form, entity);
        await repository.UpdateAsync(entity);

        if (entity.MileageKm != previousMileage)
        {
            await db.Reservations
                .Where(r => r.VehicleId == id && r.MileageUpdateSuggested)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.MileageUpdateSuggested, false));
        }

        return NavContext.RedirectAfterVehicleSave(this, id, returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Details(int id, string? returnUrl)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji vozila";
        NavContext.ApplyVehicle(this, returnUrl, model.RegistrationNumber);
        if (ViewData["Breadcrumbs"] is null)
            ViewData["Breadcrumbs"] = BreadcrumbHelper.Build("Home", "Vozila", model.RegistrationNumber);
        ViewData["MainImageUrl"] = VehicleImageHelper.GetDisplayUrl(model);
        ViewData["HasCustomMainImage"] = VehicleImageHelper.HasCustomImage(model);
        return View(model);
    }

    [Route("reg/{registration}")]
    public async Task<IActionResult> ByRegistration(string registration)
    {
        var model = await repository.GetByRegistrationAsync(registration);
        if (model is null) return NotFound();
        return RedirectToAction(nameof(Details), new { id = model.Id, returnUrl = Request.Query["returnUrl"].FirstOrDefault() });
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadAttachment(int vehicleId, IFormFile file)
    {
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId))
            return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Datoteka nije poslana." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { error = "Datoteka je prevelika (max 10 MB)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { error = "Nedozvoljena ekstenzija datoteke." });

        var uploadsPath = Path.Combine(env.WebRootPath, "uploads", "vehicles", vehicleId.ToString());
        Directory.CreateDirectory(uploadsPath);

        var storedName = Guid.NewGuid().ToString("N") + ext;
        var physicalPath = Path.Combine(uploadsPath, storedName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new VehicleAttachment
        {
            VehicleId = vehicleId,
            FileName = file.FileName,
            FilePath = $"/uploads/vehicles/{vehicleId}/{storedName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        db.VehicleAttachments.Add(attachment);
        await db.SaveChangesAsync();
        return Json(new { success = true, id = attachment.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAttachments(int vehicleId)
    {
        var attachments = await db.VehicleAttachments.AsNoTracking()
            .Where(a => a.VehicleId == vehicleId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return PartialView("_AttachmentList", attachments);
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await db.VehicleAttachments.FindAsync(id);
        if (attachment is null) return NotFound();

        var physicalPath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);

        db.VehicleAttachments.Remove(attachment);
        await db.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadMainImage(int vehicleId, IFormFile file)
    {
        var vehicle = await db.Vehicles.FindAsync(vehicleId);
        if (vehicle is null) return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Slika nije poslana." });

        if (file.Length > VehicleImageHelper.MaxImageSize)
            return BadRequest(new { error = "Slika je prevelika (max 5 MB)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!VehicleImageHelper.IsAllowedImageExtension(ext))
            return BadRequest(new { error = "Dozvoljeni formati: JPG, PNG, WEBP." });

        var uploadsPath = Path.Combine(env.WebRootPath, "uploads", "vehicles", vehicleId.ToString());
        Directory.CreateDirectory(uploadsPath);

        if (!string.IsNullOrWhiteSpace(vehicle.MainImagePath))
        {
            var oldPath = Path.Combine(env.WebRootPath, vehicle.MainImagePath.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        foreach (var old in Directory.Exists(uploadsPath) ? Directory.GetFiles(uploadsPath, "main.*") : [])
            System.IO.File.Delete(old);

        var storedName = VehicleImageHelper.MainImageFileName(ext);
        var physicalPath = Path.Combine(uploadsPath, storedName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await file.CopyToAsync(stream);

        vehicle.MainImagePath = $"/uploads/vehicles/{vehicleId}/{storedName}";
        await db.SaveChangesAsync();

        return Json(new { success = true, imageUrl = vehicle.MainImagePath });
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteMainImage(int vehicleId)
    {
        var vehicle = await db.Vehicles.FindAsync(vehicleId);
        if (vehicle is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(vehicle.MainImagePath))
        {
            var physicalPath = Path.Combine(env.WebRootPath, vehicle.MainImagePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            vehicle.MainImagePath = null;
            await db.SaveChangesAsync();
        }

        return Json(new { success = true, imageUrl = VehicleImageHelper.GetDisplayUrl(vehicle) });
    }
}

public sealed class CustomerController(CustomerRepository repository) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novi kupac";
        return View(new CustomerFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CustomerFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Customer();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi kupca";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
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

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
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

public sealed class ReservationController(ReservationRepository repository, VehicleRepository vehicles) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(int? vehicleId, string? returnUrl)
    {
        var vm = new ReservationFormVm();
        if (vehicleId is > 0)
        {
            var vehicle = await vehicles.GetByIdAsync(vehicleId.Value);
            if (vehicle is not null)
            {
                vm.VehicleId = vehicle.Id;
                vm.VehicleDisplay = $"{vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber})";
            }
        }

        ViewData["Title"] = "Nova rezervacija";
        ViewData["ReturnUrl"] = returnUrl;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(ReservationFormVm model)
    {
        var returnUrl = NavContext.GetReturnUrlFromForm(Request.Form);
        await ValidateSchedulingAsync(model);
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var entity = new Reservation();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyReservationLifecycle(entity);
        await repository.AddAsync(entity);
        return NavContext.RedirectToReturnUrl(this, returnUrl, RedirectToAction(nameof(Index)));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var full = await repository.GetByIdAsync(id);
        if (full is null) return NotFound();
        ViewData["Title"] = "Uredi rezervaciju";
        ViewData["ReturnUrl"] = returnUrl;
        return View(EntityMappers.ToForm(full));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditPost(int id, ReservationFormVm model)
    {
        if (id != model.Id) return BadRequest();
        var returnUrl = NavContext.GetReturnUrlFromForm(Request.Form);
        await ValidateSchedulingAsync(model, id);
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("Edit", model);
        }

        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyReservationLifecycle(entity);
        await repository.UpdateAsync(entity);
        return NavContext.RedirectToReturnUrl(this, returnUrl, RedirectToAction(nameof(Index)));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [Route("rezervacije/pregled/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetByIdAsync(id);
        if (model is null) return NotFound();
        ViewData["Title"] = "Detalji rezervacije";
        return View(model);
    }

    private async Task ValidateSchedulingAsync(ReservationFormVm model, int excludeReservationId = 0)
    {
        if (model.EndDate <= model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "Datum završetka mora biti nakon početka.");
            return;
        }

        if (model.VehicleId <= 0)
            return;

        var vehicle = await vehicles.GetByIdAsync(model.VehicleId);
        if (vehicle is { BlockedByService: true })
        {
            ModelState.AddModelError(
                nameof(model.VehicleId),
                "Vozilo je na servisu i privremeno je nedostupno za nove rezervacije.");
        }

        if (vehicle is { IsActive: false, BlockedByService: false })
        {
            ModelState.AddModelError(
                nameof(model.VehicleId),
                "Vozilo nije aktivno u voznom parku.");
        }

        var conflict = await repository.FindSchedulingConflictAsync(
            model.VehicleId,
            model.StartDate,
            model.EndDate,
            excludeReservationId);

        if (conflict is not null)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Vozilo je već rezervirano u odabranom periodu (rezervacija #{conflict.Id}, {conflict.StartDate:dd.MM.yyyy} – {conflict.EndDate:dd.MM.yyyy}).");
        }
    }
}

public sealed class AddonController(AddonRepository repository) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novi dodatak";
        return View(new AddonFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(AddonFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Addon();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Uredi dodatak";
        return View(EntityMappers.ToForm(entity));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
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

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(int? vehicleId, string? returnUrl)
    {
        var vm = new ServiceRecordFormVm();
        if (vehicleId is > 0)
        {
            var vehicle = await vehicles.GetByIdAsync(vehicleId.Value);
            if (vehicle is not null)
            {
                vm.VehicleId = vehicle.Id;
                vm.VehicleDisplay = $"{vehicle.RegistrationNumber} - {vehicle.Brand} {vehicle.Model}";
            }
        }

        ViewData["Title"] = "Novi servis";
        ViewData["ReturnUrl"] = returnUrl;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(ServiceRecordFormVm model)
    {
        var returnUrl = NavContext.GetReturnUrlFromForm(Request.Form);
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var entity = new ServiceRecord();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyServiceLifecycle(entity);
        await repository.AddAsync(entity);
        return NavContext.RedirectToReturnUrl(this, returnUrl, RedirectToAction(nameof(Index)));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var vehicle = await vehicles.GetByIdAsync(entity.VehicleId);
        var label = vehicle is null ? string.Empty : $"{vehicle.RegistrationNumber} - {vehicle.Brand} {vehicle.Model}";
        ViewData["Title"] = "Uredi servis";
        ViewData["ReturnUrl"] = returnUrl;
        return View(EntityMappers.ToForm(entity, label));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditPost(int id, ServiceRecordFormVm model)
    {
        if (id != model.Id) return BadRequest();
        var returnUrl = NavContext.GetReturnUrlFromForm(Request.Form);
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("Edit", model);
        }

        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        EntityMappers.Apply(model, entity);
        FleetLifecycleRules.ApplyServiceLifecycle(entity);
        await repository.UpdateAsync(entity);
        return NavContext.RedirectToReturnUrl(this, returnUrl, RedirectToAction(nameof(Index)));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() => View(await repository.GetAllAsync());

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchRows(string? q)
        => PartialView("_IndexRows", await repository.GetAllAsync(q));

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novi zaposlenik";
        return View(new EmployeeFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(EmployeeFormVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Employee();
        EntityMappers.Apply(model, entity);
        await repository.AddAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.GetTrackedAsync(id);
        if (entity is null) return NotFound();
        var branch = await branches.GetTrackedAsync(entity.BranchOfficeId);
        ViewData["Title"] = "Uredi zaposlenika";
        return View(EntityMappers.ToForm(entity, branch?.Name));
    }

    [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken, Authorize(Roles = "Admin,Manager")]
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

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
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
