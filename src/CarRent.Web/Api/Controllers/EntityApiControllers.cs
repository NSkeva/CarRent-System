using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Api.Dtos;
using CarRent.Web.Api.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Api.Controllers;

[Route("api/branch-office")]
[ApiController]
[Authorize]
public sealed class BranchOfficeApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchOfficeDto>>> Get([FromQuery] string? q)
    {
        var query = db.BranchOffices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Address.Contains(q) || x.Phone.Contains(q));
        var items = await query.OrderBy(x => x.Name).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<BranchOfficeDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BranchOfficeDto>> GetById(int id)
    {
        var entity = await db.BranchOffices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BranchOfficeDto>> Post([FromBody] BranchOfficeCreateDto model)
    {
        var entity = new BranchOffice();
        ApiMappers.Apply(model, entity);
        db.BranchOffices.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BranchOfficeDto>> Put(int id, [FromBody] BranchOfficeUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.BranchOffices.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.BranchOffices.Include(x => x.Vehicles).FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        if (entity.Vehicles.Count > 0) return BadRequest(new { error = "Poslovnica ima povezana vozila." });
        db.BranchOffices.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/vehicle")]
[ApiController]
[Authorize]
public sealed class VehicleApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> Get([FromQuery] string? q)
    {
        IQueryable<Vehicle> query = db.Vehicles.AsNoTracking().Include(v => v.BranchOffice);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(v => v.Brand.Contains(q) || v.Model.Contains(q) || v.RegistrationNumber.Contains(q));
        var items = await query.OrderBy(v => v.RegistrationNumber).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<VehicleDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var entity = await db.Vehicles.AsNoTracking().Include(v => v.BranchOffice).FirstOrDefaultAsync(v => v.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<VehicleDto>> Post([FromBody] VehicleCreateDto model)
    {
        var entity = new Vehicle();
        ApiMappers.Apply(model, entity);
        db.Vehicles.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<VehicleDto>> Put(int id, [FromBody] VehicleUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.Vehicles.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Vehicles.FindAsync(id);
        if (entity is null) return NotFound();
        db.Vehicles.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/customer")]
[ApiController]
[Authorize]
public sealed class CustomerApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Get([FromQuery] string? q)
    {
        var query = db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.FirstName.Contains(q) || c.LastName.Contains(q) || c.Email.Contains(q));
        var items = await query.OrderBy(c => c.LastName).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<CustomerDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var entity = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CustomerDto>> Post([FromBody] CustomerCreateDto model)
    {
        var entity = new Customer { CreatedAt = DateTime.UtcNow };
        ApiMappers.Apply(model, entity);
        db.Customers.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CustomerDto>> Put(int id, [FromBody] CustomerUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.Customers.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Customers.FindAsync(id);
        if (entity is null) return NotFound();
        db.Customers.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/reservation")]
[ApiController]
[Authorize]
public sealed class ReservationApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> Get([FromQuery] string? q)
    {
        IQueryable<Reservation> query = db.Reservations.AsNoTracking().Include(r => r.Customer).Include(r => r.Vehicle);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r =>
                (r.Customer != null && (r.Customer.FirstName.Contains(q) || r.Customer.LastName.Contains(q))) ||
                (r.Vehicle != null && r.Vehicle.RegistrationNumber.Contains(q)));
        var items = await query.OrderByDescending(r => r.StartDate).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<ReservationDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id)
    {
        var entity = await db.Reservations.AsNoTracking()
            .Include(r => r.Customer).Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ReservationDto>> Post([FromBody] ReservationCreateDto model)
    {
        if (model.EndDate < model.StartDate)
            return BadRequest(new { error = "Datum završetka mora biti nakon početka." });
        var entity = new Reservation { CreatedAt = DateTime.UtcNow };
        ApiMappers.Apply(model, entity);
        db.Reservations.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ReservationDto>> Put(int id, [FromBody] ReservationUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        if (model.EndDate < model.StartDate)
            return BadRequest(new { error = "Datum završetka mora biti nakon početka." });
        var entity = await db.Reservations.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Reservations.FindAsync(id);
        if (entity is null) return NotFound();
        db.Reservations.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/addon")]
[ApiController]
[Authorize]
public sealed class AddonApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddonDto>>> Get([FromQuery] string? q)
    {
        var query = db.Addons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Name.Contains(q));
        var items = await query.OrderBy(a => a.Name).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<AddonDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AddonDto>> GetById(int id)
    {
        var entity = await db.Addons.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AddonDto>> Post([FromBody] AddonCreateDto model)
    {
        var entity = new Addon();
        ApiMappers.Apply(model, entity);
        db.Addons.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AddonDto>> Put(int id, [FromBody] AddonUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.Addons.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Addons.FindAsync(id);
        if (entity is null) return NotFound();
        db.Addons.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/service-record")]
[ApiController]
[Authorize]
public sealed class ServiceRecordApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRecordDto>>> Get([FromQuery] string? q)
    {
        IQueryable<ServiceRecord> query = db.ServiceRecords.AsNoTracking().Include(s => s.Vehicle);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Description.Contains(q) || (s.Vehicle != null && s.Vehicle.RegistrationNumber.Contains(q)));
        var items = await query.OrderByDescending(s => s.ServiceDate).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<ServiceRecordDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRecordDto>> GetById(int id)
    {
        var entity = await db.ServiceRecords.AsNoTracking().Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ServiceRecordDto>> Post([FromBody] ServiceRecordCreateDto model)
    {
        var entity = new ServiceRecord();
        ApiMappers.Apply(model, entity);
        db.ServiceRecords.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ServiceRecordDto>> Put(int id, [FromBody] ServiceRecordUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.ServiceRecords.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.ServiceRecords.FindAsync(id);
        if (entity is null) return NotFound();
        db.ServiceRecords.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/employee")]
[ApiController]
[Authorize]
public sealed class EmployeeApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> Get([FromQuery] string? q)
    {
        IQueryable<Employee> query = db.Employees.AsNoTracking().Include(e => e.BranchOffice);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(e => e.FirstName.Contains(q) || e.LastName.Contains(q) || e.JobTitle.Contains(q));
        var items = await query.OrderBy(e => e.LastName).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<EmployeeDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var entity = await db.Employees.AsNoTracking().Include(e => e.BranchOffice).FirstOrDefaultAsync(e => e.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmployeeDto>> Post([FromBody] EmployeeCreateDto model)
    {
        var entity = new Employee();
        ApiMappers.Apply(model, entity);
        db.Employees.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmployeeDto>> Put(int id, [FromBody] EmployeeUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.Employees.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Employees.FindAsync(id);
        if (entity is null) return NotFound();
        db.Employees.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}

[Route("api/partner")]
[ApiController]
[Authorize]
public sealed class PartnerApiController(CarRentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartnerDto>>> Get([FromQuery] string? q)
    {
        var query = db.Partners.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.CompanyName.Contains(q) || p.ContactPerson.Contains(q) || p.Email.Contains(q));
        var items = await query.OrderBy(p => p.CompanyName).ToListAsync();
        return Ok(items.Select(ApiMappers.ToDto));
    }

    [HttpGet("search/{q}")]
    public Task<ActionResult<IEnumerable<PartnerDto>>> Search(string q) => Get(q);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartnerDto>> GetById(int id)
    {
        var entity = await db.Partners.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return entity is null ? NotFound() : Ok(ApiMappers.ToDto(entity));
    }

    [HttpPost, Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PartnerDto>> Post([FromBody] PartnerCreateDto model)
    {
        var entity = new Partner();
        ApiMappers.Apply(model, entity);
        db.Partners.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiMappers.ToDto(entity));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PartnerDto>> Put(int id, [FromBody] PartnerUpdateDto model)
    {
        if (id != model.Id) return BadRequest();
        var entity = await db.Partners.FindAsync(id);
        if (entity is null) return NotFound();
        ApiMappers.Apply(model, entity);
        await db.SaveChangesAsync();
        return Ok(ApiMappers.ToDto(entity));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Partners.FindAsync(id);
        if (entity is null) return NotFound();
        db.Partners.Remove(entity);
        await db.SaveChangesAsync();
        return Ok();
    }
}
