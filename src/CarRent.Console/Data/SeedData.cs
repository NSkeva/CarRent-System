using CarRent.Console.Models.Entities;
using CarRent.Console.Models.Enums;

namespace CarRent.Console.Data;

public class SeedResult
{
    public List<BranchOffice> Branches { get; init; } = new();
    public List<Vehicle> Vehicles { get; init; } = new();
    public List<Customer> Customers { get; init; } = new();
    public List<Reservation> Reservations { get; init; } = new();
    public List<ServiceRecord> ServiceRecords { get; init; } = new();
    public List<Addon> Addons { get; init; } = new();
}

public static class SeedData
{
    public static SeedResult Create()
    {
        var now = DateTime.UtcNow;

        var branches = new List<BranchOffice>
        {
            new() { Id = 1, Name = "Zagreb Centar", LocationType = LocationType.Downtown, Address = "Ilica 1", Phone = "+385111111" },
            new() { Id = 2, Name = "Zagreb Airport", LocationType = LocationType.Airport, Address = "Airport Rd 10", Phone = "+385222222" },
            new() { Id = 3, Name = "Split Riva", LocationType = LocationType.MainOffice, Address = "Riva 5", Phone = "+385333333" }
        };

        var vehicles = new List<Vehicle>
        {
            new() { Id = 1, RegistrationNumber = "ZG-100-AA", Brand = "Skoda", Model = "Octavia", Year = 2021, Type = VehicleType.Car, MileageKm = 75500, IsActive = true, DailyPrice = 55m, BranchOfficeId = 1 },
            new() { Id = 2, RegistrationNumber = "ZG-200-BB", Brand = "Volkswagen", Model = "Golf", Year = 2020, Type = VehicleType.Car, MileageKm = 91400, IsActive = true, DailyPrice = 50m, BranchOfficeId = 1 },
            new() { Id = 3, RegistrationNumber = "ZG-300-CC", Brand = "Renault", Model = "Trafic", Year = 2019, Type = VehicleType.Van, MileageKm = 126000, IsActive = true, DailyPrice = 79m, BranchOfficeId = 2 },
            new() { Id = 4, RegistrationNumber = "ST-400-DD", Brand = "Piaggio", Model = "Liberty", Year = 2023, Type = VehicleType.Scooter, MileageKm = 8300, IsActive = true, DailyPrice = 30m, BranchOfficeId = 3 },
            new() { Id = 5, RegistrationNumber = "ST-500-EE", Brand = "Yamaha", Model = "MT-07", Year = 2022, Type = VehicleType.Motorcycle, MileageKm = 14500, IsActive = true, DailyPrice = 64m, BranchOfficeId = 3 },
            new() { Id = 6, RegistrationNumber = "ZG-600-FF", Brand = "Cube", Model = "Touring", Year = 2024, Type = VehicleType.Bicycle, MileageKm = 1900, IsActive = true, DailyPrice = 18m, BranchOfficeId = 2 }
        };

        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "Marko", LastName = "Horvat", Email = "marko@example.com", Phone = "+38591000111", DateOfBirth = new DateTime(1994, 5, 4), DriverLicenseNumber = "HR001", CreatedAt = now.AddMonths(-14) },
            new() { Id = 2, FirstName = "Iva", LastName = "Peric", Email = "iva@example.com", Phone = "+38591000222", DateOfBirth = new DateTime(1998, 11, 10), DriverLicenseNumber = "HR002", CreatedAt = now.AddMonths(-8) },
            new() { Id = 3, FirstName = "Luka", LastName = "Maric", Email = "luka@example.com", Phone = "+38591000333", DateOfBirth = new DateTime(1989, 8, 18), DriverLicenseNumber = "HR003", CreatedAt = now.AddMonths(-20) },
            new() { Id = 4, FirstName = "Ana", LastName = "Kovacic", Email = "ana@example.com", Phone = "+38591000444", DateOfBirth = new DateTime(2000, 2, 2), DriverLicenseNumber = "HR004", CreatedAt = now.AddMonths(-3) }
        };

        var addons = new List<Addon>
        {
            new() { Id = 1, Name = "GPS", PricePerDay = 4m },
            new() { Id = 2, Name = "Djecja sjedalica", PricePerDay = 6m },
            new() { Id = 3, Name = "Premium osiguranje", PricePerDay = 12m }
        };

        var reservations = new List<Reservation>
        {
            new() { Id = 1, CustomerId = 1, VehicleId = 1, StartDate = now.Date.AddDays(-1), EndDate = now.Date.AddDays(2), PickupLocation = LocationType.Downtown, DropoffLocation = LocationType.Airport, Status = ReservationStatus.Active, BasePrice = 165m, CreatedAt = now.AddDays(-9) },
            new() { Id = 2, CustomerId = 2, VehicleId = 3, StartDate = now.Date.AddDays(3), EndDate = now.Date.AddDays(7), PickupLocation = LocationType.Airport, DropoffLocation = LocationType.MainOffice, Status = ReservationStatus.Confirmed, BasePrice = 316m, CreatedAt = now.AddDays(-2) },
            new() { Id = 3, CustomerId = 3, VehicleId = 4, StartDate = now.Date, EndDate = now.Date.AddDays(1), PickupLocation = LocationType.MainOffice, DropoffLocation = LocationType.MainOffice, Status = ReservationStatus.Active, BasePrice = 30m, CreatedAt = now.AddDays(-1) },
            new() { Id = 4, CustomerId = 4, VehicleId = 2, StartDate = now.Date.AddDays(5), EndDate = now.Date.AddDays(9), PickupLocation = LocationType.Downtown, DropoffLocation = LocationType.Downtown, Status = ReservationStatus.Confirmed, BasePrice = 200m, CreatedAt = now.AddHours(-20) },
            new() { Id = 5, CustomerId = 1, VehicleId = 5, StartDate = now.Date.AddDays(-6), EndDate = now.Date.AddDays(-2), PickupLocation = LocationType.MainOffice, DropoffLocation = LocationType.HotelPartner, Status = ReservationStatus.Completed, BasePrice = 256m, CreatedAt = now.AddDays(-14) },
            new() { Id = 6, CustomerId = 2, VehicleId = 6, StartDate = now.Date.AddDays(1), EndDate = now.Date.AddDays(3), PickupLocation = LocationType.Airport, DropoffLocation = LocationType.Downtown, Status = ReservationStatus.Confirmed, BasePrice = 36m, CreatedAt = now.AddHours(-8) }
        };

        reservations[0].Addons.Add(new ReservationAddon { ReservationId = 1, AddonId = 1, AddonName = "GPS", PriceAtReservation = 4m, Quantity = 3 });
        reservations[1].Addons.Add(new ReservationAddon { ReservationId = 2, AddonId = 3, AddonName = "Premium osiguranje", PriceAtReservation = 12m, Quantity = 4 });
        reservations[2].Addons.Add(new ReservationAddon { ReservationId = 3, AddonId = 2, AddonName = "Djecja sjedalica", PriceAtReservation = 6m, Quantity = 1 });
        reservations[3].Addons.Add(new ReservationAddon { ReservationId = 4, AddonId = 1, AddonName = "GPS", PriceAtReservation = 4m, Quantity = 4 });

        var services = new List<ServiceRecord>
        {
            new() { Id = 1, VehicleId = 1, ServiceDate = now.Date.AddDays(-40), Status = ServiceStatus.Completed, Description = "Mali servis", MileageAtService = 73000, Cost = 240m, NextRecommendedServiceDate = now.Date.AddDays(60) },
            new() { Id = 2, VehicleId = 2, ServiceDate = now.Date.AddDays(-22), Status = ServiceStatus.Completed, Description = "Zamjena guma", MileageAtService = 90000, Cost = 300m, NextRecommendedServiceDate = now.Date.AddDays(80) },
            new() { Id = 3, VehicleId = 3, ServiceDate = now.Date.AddDays(-15), Status = ServiceStatus.InProgress, Description = "Kocnice", MileageAtService = 125500, Cost = 410m, NextRecommendedServiceDate = now.Date.AddDays(30) },
            new() { Id = 4, VehicleId = 4, ServiceDate = now.Date.AddDays(-5), Status = ServiceStatus.Completed, Description = "Pregled skutera", MileageAtService = 8100, Cost = 90m, NextRecommendedServiceDate = now.Date.AddDays(120) },
            new() { Id = 5, VehicleId = 5, ServiceDate = now.Date.AddDays(10), Status = ServiceStatus.Planned, Description = "Lanac i ulje", MileageAtService = 14500, Cost = 180m, NextRecommendedServiceDate = now.Date.AddDays(190) }
        };

        foreach (var branch in branches)
        {
            branch.Vehicles = vehicles.Where(v => v.BranchOfficeId == branch.Id).ToList();
        }

        foreach (var customer in customers)
        {
            customer.Reservations = reservations.Where(r => r.CustomerId == customer.Id).ToList();
        }

        foreach (var vehicle in vehicles)
        {
            vehicle.ServiceRecords = services.Where(s => s.VehicleId == vehicle.Id).ToList();
        }

        return new SeedResult
        {
            Branches = branches,
            Vehicles = vehicles,
            Customers = customers,
            Reservations = reservations,
            ServiceRecords = services,
            Addons = addons
        };
    }
}
