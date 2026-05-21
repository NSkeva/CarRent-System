using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Model.Enums;
using CarRent.Web.Services;
using CarRent.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Web.Repositories;

public sealed class BranchOfficeRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<BranchOffice>> GetAllAsync(string? q = null)
    {
        var query = db.BranchOffices.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Address.Contains(q) || x.Phone.Contains(q));
        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<BranchOffice?> GetByIdAsync(int id)
        => await db.BranchOffices.Include(b => b.Vehicles).AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<BranchOffice?> GetTrackedAsync(int id) => await db.BranchOffices.FindAsync(id);

    public async Task<IReadOnlyList<LookupItemVm>> SearchLookupAsync(string? q, int take = 15)
        => await db.BranchOffices.AsNoTracking()
            .Where(b => string.IsNullOrWhiteSpace(q) || b.Name.Contains(q) || b.Address.Contains(q))
            .OrderBy(b => b.Name).Take(take)
            .Select(b => new LookupItemVm { Id = b.Id, Label = b.Name })
            .ToListAsync();

    public async Task AddAsync(BranchOffice entity) { db.BranchOffices.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(BranchOffice entity) { db.BranchOffices.Update(entity); await db.SaveChangesAsync(); }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await db.BranchOffices.Include(b => b.Vehicles).FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null) return false;
        if (entity.Vehicles.Count > 0) return false;
        db.BranchOffices.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}

public sealed class VehicleRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(string? q = null)
    {
        var query = db.Vehicles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(v => v.Brand.Contains(q) || v.Model.Contains(q) || v.RegistrationNumber.Contains(q));
        return await query.OrderBy(v => v.RegistrationNumber).ToListAsync();
    }

    public async Task<IReadOnlyList<LookupItemVm>> SearchLookupAsync(string? q, int take = 15)
        => await db.Vehicles.AsNoTracking()
            .Where(v => string.IsNullOrWhiteSpace(q) || v.Brand.Contains(q) || v.Model.Contains(q) || v.RegistrationNumber.Contains(q))
            .OrderBy(v => v.RegistrationNumber).Take(take)
            .Select(v => new LookupItemVm { Id = v.Id, Label = v.RegistrationNumber + " - " + v.Brand + " " + v.Model })
            .ToListAsync();

    public async Task<Vehicle?> GetByIdAsync(int id)
        => await db.Vehicles
            .Include(v => v.ServiceRecords)
            .Include(v => v.Reservations).ThenInclude(r => r.Customer)
            .Include(v => v.BranchOffice)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vehicle?> GetByRegistrationAsync(string registration)
        => await db.Vehicles.Include(v => v.ServiceRecords).Include(v => v.BranchOffice).AsNoTracking()
            .FirstOrDefaultAsync(v => v.RegistrationNumber == registration);

    public async Task<Vehicle?> GetTrackedAsync(int id) => await db.Vehicles.FindAsync(id);
    public async Task AddAsync(Vehicle entity) { db.Vehicles.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Vehicle entity) { db.Vehicles.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.Vehicles.FindAsync(id); if (e is null) return; db.Vehicles.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class CustomerRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? q = null)
    {
        var query = db.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.FirstName.Contains(q) || c.LastName.Contains(q) || c.Email.Contains(q));
        return await query.OrderBy(c => c.LastName).ToListAsync();
    }

    public async Task<IReadOnlyList<LookupItemVm>> SearchLookupAsync(string? q, int take = 15)
        => await db.Customers.AsNoTracking()
            .Where(c => string.IsNullOrWhiteSpace(q) || c.FirstName.Contains(q) || c.LastName.Contains(q) || c.Email.Contains(q))
            .OrderBy(c => c.LastName).Take(take)
            .Select(c => new LookupItemVm { Id = c.Id, Label = c.FirstName + " " + c.LastName + " (" + c.Email + ")" })
            .ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id)
        => await db.Customers.Include(c => c.Reservations).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Customer?> GetTrackedAsync(int id) => await db.Customers.FindAsync(id);
    public async Task AddAsync(Customer entity) { db.Customers.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Customer entity) { db.Customers.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.Customers.FindAsync(id); if (e is null) return; db.Customers.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class ReservationRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Reservation>> GetAllAsync(string? q = null)
    {
        var query = db.Reservations.Include(r => r.Customer).Include(r => r.Vehicle).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q) && int.TryParse(q, out var id))
            query = query.Where(r => r.Id == id);
        else if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.Status.ToString().Contains(q));
        return await query.OrderByDescending(r => r.StartDate).ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
        => await db.Reservations.Include(r => r.Addons).Include(r => r.Customer).Include(r => r.Vehicle).AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Reservation?> GetTrackedAsync(int id) => await db.Reservations.FindAsync(id);
    public async Task AddAsync(Reservation entity) { entity.CreatedAt = DateTime.UtcNow; db.Reservations.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Reservation entity) { db.Reservations.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.Reservations.FindAsync(id); if (e is null) return; db.Reservations.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class AddonRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Addon>> GetAllAsync(string? q = null)
    {
        var query = db.Addons.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(a => a.Name.Contains(q));
        return await query.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<Addon?> GetByIdAsync(int id) => await db.Addons.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    public async Task<Addon?> GetTrackedAsync(int id) => await db.Addons.FindAsync(id);
    public async Task AddAsync(Addon entity) { db.Addons.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Addon entity) { db.Addons.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.Addons.FindAsync(id); if (e is null) return; db.Addons.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class ServiceRecordRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<ServiceRecord>> GetAllAsync(string? q = null)
    {
        var query = db.ServiceRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(s => s.Description.Contains(q));
        return await query.OrderByDescending(s => s.ServiceDate).ToListAsync();
    }

    public async Task<ServiceRecord?> GetByIdAsync(int id) => await db.ServiceRecords.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    public async Task<ServiceRecord?> GetTrackedAsync(int id) => await db.ServiceRecords.FindAsync(id);
    public async Task AddAsync(ServiceRecord entity) { db.ServiceRecords.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(ServiceRecord entity) { db.ServiceRecords.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.ServiceRecords.FindAsync(id); if (e is null) return; db.ServiceRecords.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class EmployeeRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(string? q = null)
    {
        var query = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(e => e.FirstName.Contains(q) || e.LastName.Contains(q) || e.JobTitle.Contains(q));
        return await query.OrderBy(e => e.LastName).ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id) => await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    public async Task<Employee?> GetTrackedAsync(int id) => await db.Employees.FindAsync(id);
    public async Task AddAsync(Employee entity) { db.Employees.Add(entity); await db.SaveChangesAsync(); }
    public async Task UpdateAsync(Employee entity) { db.Employees.Update(entity); await db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await db.Employees.FindAsync(id); if (e is null) return; db.Employees.Remove(e); await db.SaveChangesAsync(); }
}

public sealed class PartnerRepository(CarRentDbContext db)
{
    public async Task<IReadOnlyList<Partner>> GetAllAsync(string? q = null)
    {
        var query = db.Partners.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.CompanyName.Contains(q) || p.ContactPerson.Contains(q) || p.Email.Contains(q));
        return await query.OrderBy(p => p.CompanyName).ToListAsync();
    }

    public async Task<Partner?> GetByIdAsync(int id)
        => await db.Partners.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Partner?> GetTrackedAsync(int id) => await db.Partners.FindAsync(id);

    public async Task AddAsync(Partner partner)
    {
        db.Partners.Add(partner);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Partner partner)
    {
        db.Partners.Update(partner);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await db.Partners.FindAsync(id);
        if (entity is null) return;
        db.Partners.Remove(entity);
        await db.SaveChangesAsync();
    }
}

public sealed class DashboardRepository(CarRentDbContext db)
{
    public async Task<HomeIndexVm> BuildHomeVmAsync()
    {
        var activeReservations = await db.Reservations.CountAsync(r =>
            r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Confirmed);
        var servicesSoon = await db.ServiceRecords.CountAsync(s =>
            s.NextRecommendedServiceDate != null &&
            s.NextRecommendedServiceDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daily = await BuildDailyPlanVmAsync(today);

        return new HomeIndexVm
        {
            TotalVehicles = await db.Vehicles.CountAsync(),
            ActiveReservations = activeReservations,
            Branches = await db.BranchOffices.CountAsync(),
            ServicesDueSoon = servicesSoon,
            TodayDepartures = daily.Departures,
            TodayReturns = daily.Returns,
            QuickLinks =
            [
                new() { Title = "Timeline", Subtitle = "Mjesečni raspored po vozilima", Controller = "Timeline", Action = "Index" },
                new() { Title = "Dnevni plan", Subtitle = "Današnji odlazak i povrat", Controller = "DailyPlan", Action = "Index" },
                new() { Title = "Vozni park", Subtitle = "Kartice i servisne akcije", Controller = "Fleet", Action = "Index" },
                new() { Title = "Rezervacije", Subtitle = "Lista i nova rezervacija", Controller = "Reservation", Action = "Index" }
            ]
        };
    }

    public async Task<TimelineVm> BuildTimelineVmAsync(DateOnly month, string? q, string? vehicleType, string? reservationStatus)
    {
        var vehiclesQuery = db.Vehicles.AsNoTracking().AsQueryable();
        var reservationsQuery = db.Reservations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            vehiclesQuery = vehiclesQuery.Where(v =>
                (v.Brand + " " + v.Model + " " + v.RegistrationNumber).Contains(q));
        }

        if (Enum.TryParse<VehicleType>(vehicleType, out var parsedVehicleType))
            vehiclesQuery = vehiclesQuery.Where(v => v.Type == parsedVehicleType);

        if (Enum.TryParse<ReservationStatus>(reservationStatus, out var parsedReservationStatus))
            reservationsQuery = reservationsQuery.Where(r => r.Status == parsedReservationStatus);

        var vehicles = await vehiclesQuery.Include(v => v.BranchOffice).ToListAsync();
        var reservations = await reservationsQuery.Include(r => r.Customer).ToListAsync();

        var days = Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(d => new DateOnly(month.Year, month.Month, d))
            .ToList();

        var reservationsByVehicle = reservations.GroupBy(r => r.VehicleId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = vehicles.Select(v =>
        {
            reservationsByVehicle.TryGetValue(v.Id, out var vehicleReservations);
            vehicleReservations ??= [];
            return new TimelineRowVm
            {
                Vehicle = v,
                BranchName = v.BranchOffice?.Name,
                Bars = TimelineLayoutHelper.BuildBars(vehicleReservations, month),
                Cells = days.Select(day => new TimelineCellVm
                {
                    Day = day,
                    Reservations = vehicleReservations.Where(r =>
                        DateOnly.FromDateTime(r.StartDate) <= day &&
                        DateOnly.FromDateTime(r.EndDate) >= day).ToList()
                }).ToList()
            };
        }).ToList();

        return new TimelineVm
        {
            Month = month,
            SearchText = q ?? string.Empty,
            VehicleType = vehicleType,
            ReservationStatus = reservationStatus,
            VehicleTypes = Enum.GetValues<VehicleType>(),
            ReservationStatuses = Enum.GetValues<ReservationStatus>(),
            Days = days,
            Rows = rows
        };
    }

    public async Task<DailyPlanVm> BuildDailyPlanVmAsync(DateOnly day)
    {
        var reservations = await db.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .AsNoTracking()
            .ToListAsync();

        IReadOnlyList<DailyReservationVm> Build(Func<Reservation, bool> predicate)
            => reservations.Where(predicate)
                .Select(r => new DailyReservationVm
                {
                    Reservation = r,
                    Vehicle = r.Vehicle!,
                    Customer = r.Customer!
                })
                .ToList();

        return new DailyPlanVm
        {
            Day = day,
            Returns = Build(r => DateOnly.FromDateTime(r.EndDate) == day),
            Departures = Build(r => DateOnly.FromDateTime(r.StartDate) == day)
        };
    }

    public async Task<IReadOnlyList<FleetCardVm>> BuildFleetCardsAsync()
    {
        var branches = await db.BranchOffices.AsNoTracking().ToDictionaryAsync(b => b.Id, b => b.Name);
        var vehicles = await db.Vehicles.AsNoTracking().ToListAsync();

        return vehicles.Select(v => new FleetCardVm
        {
            Vehicle = v,
            BranchName = branches.GetValueOrDefault(v.BranchOfficeId, "Nepoznato"),
            ImageUrl = $"https://picsum.photos/seed/carrent-{v.Id}/420/240"
        }).ToList();
    }
}
