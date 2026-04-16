using CarRent.Console.Data;
using CarRent.Console.Models.Entities;
using CarRent.Console.Models.Enums;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Repositories;

public sealed class SeedSnapshotRepository
{
    public SeedSnapshotRepository()
    {
        Data = SeedData.Create();
    }

    public SeedResult Data { get; }
}

public sealed class BranchOfficeRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<BranchOffice> GetAll() => snapshot.Data.Branches;
    public BranchOffice? GetById(int id) => snapshot.Data.Branches.FirstOrDefault(x => x.Id == id);
}

public sealed class VehicleRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<Vehicle> GetAll() => snapshot.Data.Vehicles;
    public Vehicle? GetById(int id) => snapshot.Data.Vehicles.FirstOrDefault(x => x.Id == id);
}

public sealed class CustomerRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<Customer> GetAll() => snapshot.Data.Customers;
    public Customer? GetById(int id) => snapshot.Data.Customers.FirstOrDefault(x => x.Id == id);
}

public sealed class ReservationRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<Reservation> GetAll() => snapshot.Data.Reservations;
    public Reservation? GetById(int id) => snapshot.Data.Reservations.FirstOrDefault(x => x.Id == id);
}

public sealed class AddonRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<Addon> GetAll() => snapshot.Data.Addons;
    public Addon? GetById(int id) => snapshot.Data.Addons.FirstOrDefault(x => x.Id == id);
}

public sealed class ServiceRecordRepository(SeedSnapshotRepository snapshot)
{
    public IReadOnlyList<ServiceRecord> GetAll() => snapshot.Data.ServiceRecords;
    public ServiceRecord? GetById(int id) => snapshot.Data.ServiceRecords.FirstOrDefault(x => x.Id == id);
}

public sealed class EmployeeRepository(SeedSnapshotRepository snapshot)
{
    private readonly List<Employee> _employees =
    [
        new() { Id = 1, FirstName = "Petra", LastName = "Barišić", JobTitle = "Voditelj poslovnice", BranchOfficeId = 1, HiredAt = DateTime.UtcNow.AddYears(-4) },
        new() { Id = 2, FirstName = "Ivan", LastName = "Matić", JobTitle = "Dispečer voznog parka", BranchOfficeId = 2, HiredAt = DateTime.UtcNow.AddYears(-2) },
        new() { Id = 3, FirstName = "Lucija", LastName = "Rajić", JobTitle = "Podrška korisnicima", BranchOfficeId = 3, HiredAt = DateTime.UtcNow.AddMonths(-15) }
    ];

    public IReadOnlyList<Employee> GetAll() => _employees;
    public Employee? GetById(int id) => _employees.FirstOrDefault(x => x.Id == id);
}

public sealed class PartnerRepository
{
    private readonly List<Partner> _partners =
    [
        new() { Id = 1, CompanyName = "Hotel Aurora", ContactPerson = "Mario Lovrić", Phone = "+38599888111", Email = "mario@aurora.hr" },
        new() { Id = 2, CompanyName = "Travel Hub Adriatic", ContactPerson = "Nina Grgić", Phone = "+38599888222", Email = "nina@travelhub.hr" },
        new() { Id = 3, CompanyName = "Airport Shuttle Plus", ContactPerson = "Filip Kranjčec", Phone = "+38599888333", Email = "filip@shuttleplus.hr" }
    ];

    public IReadOnlyList<Partner> GetAll() => _partners;
    public Partner? GetById(int id) => _partners.FirstOrDefault(x => x.Id == id);
}

public sealed class DashboardRepository(
    SeedSnapshotRepository seed,
    ReservationRepository reservationRepository,
    VehicleRepository vehicleRepository)
{
    public HomeIndexVm BuildHomeVm()
    {
        var activeReservations = reservationRepository.GetAll()
            .Count(r => r.Status is ReservationStatus.Active or ReservationStatus.Confirmed);
        var servicesSoon = seed.Data.ServiceRecords.Count(s =>
            s.NextRecommendedServiceDate is not null &&
            s.NextRecommendedServiceDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30));

        return new HomeIndexVm
        {
            TotalVehicles = vehicleRepository.GetAll().Count,
            ActiveReservations = activeReservations,
            Branches = seed.Data.Branches.Count,
            ServicesDueSoon = servicesSoon,
            QuickLinks =
            [
                new() { Title = "Timeline", Subtitle = "Mjesečni raspored po vozilima", Controller = "Timeline", Action = "Index" },
                new() { Title = "Dnevni plan", Subtitle = "Današnji odlazak i povrat", Controller = "DailyPlan", Action = "Index" },
                new() { Title = "Vozni park", Subtitle = "Kartice i servisne akcije", Controller = "Fleet", Action = "Index" },
                new() { Title = "Partneri", Subtitle = "B2B kontakti i suradnje", Controller = "Partners", Action = "Index" }
            ]
        };
    }

    public TimelineVm BuildTimelineVm(DateOnly month, string? q, string? vehicleType, string? reservationStatus)
    {
        var vehicles = vehicleRepository.GetAll().AsEnumerable();
        var reservations = reservationRepository.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            vehicles = vehicles.Where(v =>
                $"{v.Brand} {v.Model} {v.RegistrationNumber}".Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (Enum.TryParse<VehicleType>(vehicleType, out var parsedVehicleType))
        {
            vehicles = vehicles.Where(v => v.Type == parsedVehicleType);
        }

        if (Enum.TryParse<ReservationStatus>(reservationStatus, out var parsedReservationStatus))
        {
            reservations = reservations.Where(r => r.Status == parsedReservationStatus);
        }

        var days = Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(d => new DateOnly(month.Year, month.Month, d))
            .ToList();

        var reservationsByVehicle = reservations
            .GroupBy(r => r.VehicleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = vehicles.Select(v =>
        {
            reservationsByVehicle.TryGetValue(v.Id, out var vehicleReservations);
            vehicleReservations ??= [];
            return new TimelineRowVm
            {
                Vehicle = v,
                Cells = days.Select(day => new TimelineCellVm
                {
                    Day = day,
                    Reservations = vehicleReservations.Where(r =>
                            DateOnly.FromDateTime(r.StartDate) <= day &&
                            DateOnly.FromDateTime(r.EndDate) >= day)
                        .ToList()
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

    public DailyPlanVm BuildDailyPlanVm(DateOnly day)
    {
        var reservations = reservationRepository.GetAll();
        var vehicles = vehicleRepository.GetAll().ToDictionary(v => v.Id);
        var customers = seed.Data.Customers.ToDictionary(c => c.Id);

        IReadOnlyList<DailyReservationVm> Build(Func<Reservation, bool> predicate)
            => reservations.Where(predicate)
                .Select(r => new DailyReservationVm
                {
                    Reservation = r,
                    Vehicle = vehicles[r.VehicleId],
                    Customer = customers[r.CustomerId]
                })
                .ToList();

        return new DailyPlanVm
        {
            Day = day,
            Returns = Build(r => DateOnly.FromDateTime(r.EndDate) == day),
            Departures = Build(r => DateOnly.FromDateTime(r.StartDate) == day)
        };
    }

    public IReadOnlyList<FleetCardVm> BuildFleetCards()
    {
        var branches = seed.Data.Branches.ToDictionary(b => b.Id, b => b.Name);
        return vehicleRepository.GetAll().Select(v => new FleetCardVm
        {
            Vehicle = v,
            BranchName = branches.GetValueOrDefault(v.BranchOfficeId, "Nepoznato"),
            ImageUrl = $"https://picsum.photos/seed/carrent-{v.Id}/420/240"
        }).ToList();
    }
}
