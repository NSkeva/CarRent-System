using CarRent.Model.Entities;
using CarRent.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.DAL;

public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 4, 16, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<BranchOffice>().HasData(
            new BranchOffice { Id = 1, Name = "Zagreb Centar", LocationType = LocationType.Downtown, Address = "Ilica 1", Phone = "+385111111" },
            new BranchOffice { Id = 2, Name = "Zagreb Airport", LocationType = LocationType.Airport, Address = "Airport Rd 10", Phone = "+385222222" },
            new BranchOffice { Id = 3, Name = "Split Riva", LocationType = LocationType.MainOffice, Address = "Riva 5", Phone = "+385333333" });

        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle { Id = 1, RegistrationNumber = "ZG-100-AA", Brand = "Skoda", Model = "Octavia", Year = 2021, Type = VehicleType.Car, MileageKm = 75500, IsActive = true, DailyPrice = 55m, BranchOfficeId = 1 },
            new Vehicle { Id = 2, RegistrationNumber = "ZG-200-BB", Brand = "Volkswagen", Model = "Golf", Year = 2020, Type = VehicleType.Car, MileageKm = 91400, IsActive = true, DailyPrice = 50m, BranchOfficeId = 1 },
            new Vehicle { Id = 3, RegistrationNumber = "ZG-300-CC", Brand = "Renault", Model = "Trafic", Year = 2019, Type = VehicleType.Van, MileageKm = 126000, IsActive = true, DailyPrice = 79m, BranchOfficeId = 2 },
            new Vehicle { Id = 4, RegistrationNumber = "ST-400-DD", Brand = "Piaggio", Model = "Liberty", Year = 2023, Type = VehicleType.Scooter, MileageKm = 8300, IsActive = true, DailyPrice = 30m, BranchOfficeId = 3 },
            new Vehicle { Id = 5, RegistrationNumber = "ST-500-EE", Brand = "Yamaha", Model = "MT-07", Year = 2022, Type = VehicleType.Motorcycle, MileageKm = 14500, IsActive = true, DailyPrice = 64m, BranchOfficeId = 3 },
            new Vehicle { Id = 6, RegistrationNumber = "ZG-600-FF", Brand = "Cube", Model = "Touring", Year = 2024, Type = VehicleType.Bicycle, MileageKm = 1900, IsActive = true, DailyPrice = 18m, BranchOfficeId = 2 });

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FirstName = "Marko", LastName = "Horvat", Email = "marko@example.com", Phone = "+38591000111", DateOfBirth = new DateTime(1994, 5, 4), DriverLicenseNumber = "HR001", CreatedAt = now.AddMonths(-14) },
            new Customer { Id = 2, FirstName = "Iva", LastName = "Peric", Email = "iva@example.com", Phone = "+38591000222", DateOfBirth = new DateTime(1998, 11, 10), DriverLicenseNumber = "HR002", CreatedAt = now.AddMonths(-8) },
            new Customer { Id = 3, FirstName = "Luka", LastName = "Maric", Email = "luka@example.com", Phone = "+38591000333", DateOfBirth = new DateTime(1989, 8, 18), DriverLicenseNumber = "HR003", CreatedAt = now.AddMonths(-20) },
            new Customer { Id = 4, FirstName = "Ana", LastName = "Kovacic", Email = "ana@example.com", Phone = "+38591000444", DateOfBirth = new DateTime(2000, 2, 2), DriverLicenseNumber = "HR004", CreatedAt = now.AddMonths(-3) });

        modelBuilder.Entity<Addon>().HasData(
            new Addon { Id = 1, Name = "GPS", PricePerDay = 4m },
            new Addon { Id = 2, Name = "Djecja sjedalica", PricePerDay = 6m },
            new Addon { Id = 3, Name = "Premium osiguranje", PricePerDay = 12m });

        modelBuilder.Entity<Reservation>().HasData(
            new Reservation { Id = 1, CustomerId = 1, VehicleId = 1, StartDate = now.Date.AddDays(-1), EndDate = now.Date.AddDays(2), PickupLocation = LocationType.Downtown, DropoffLocation = LocationType.Airport, Status = ReservationStatus.Active, BasePrice = 165m, CreatedAt = now.AddDays(-9) },
            new Reservation { Id = 2, CustomerId = 2, VehicleId = 3, StartDate = now.Date.AddDays(3), EndDate = now.Date.AddDays(7), PickupLocation = LocationType.Airport, DropoffLocation = LocationType.MainOffice, Status = ReservationStatus.Confirmed, BasePrice = 316m, CreatedAt = now.AddDays(-2) },
            new Reservation { Id = 3, CustomerId = 3, VehicleId = 4, StartDate = now.Date, EndDate = now.Date.AddDays(1), PickupLocation = LocationType.MainOffice, DropoffLocation = LocationType.MainOffice, Status = ReservationStatus.Active, BasePrice = 30m, CreatedAt = now.AddDays(-1) },
            new Reservation { Id = 4, CustomerId = 4, VehicleId = 2, StartDate = now.Date.AddDays(5), EndDate = now.Date.AddDays(9), PickupLocation = LocationType.Downtown, DropoffLocation = LocationType.Downtown, Status = ReservationStatus.Confirmed, BasePrice = 200m, CreatedAt = now.AddHours(-20) },
            new Reservation { Id = 5, CustomerId = 1, VehicleId = 5, StartDate = now.Date.AddDays(-6), EndDate = now.Date.AddDays(-2), PickupLocation = LocationType.MainOffice, DropoffLocation = LocationType.HotelPartner, Status = ReservationStatus.Completed, BasePrice = 256m, CreatedAt = now.AddDays(-14) },
            new Reservation { Id = 6, CustomerId = 2, VehicleId = 6, StartDate = now.Date.AddDays(1), EndDate = now.Date.AddDays(3), PickupLocation = LocationType.Airport, DropoffLocation = LocationType.Downtown, Status = ReservationStatus.Confirmed, BasePrice = 36m, CreatedAt = now.AddHours(-8) });

        modelBuilder.Entity<ReservationAddon>().HasData(
            new ReservationAddon { ReservationId = 1, AddonId = 1, AddonName = "GPS", PriceAtReservation = 4m, Quantity = 3 },
            new ReservationAddon { ReservationId = 2, AddonId = 3, AddonName = "Premium osiguranje", PriceAtReservation = 12m, Quantity = 4 },
            new ReservationAddon { ReservationId = 3, AddonId = 2, AddonName = "Djecja sjedalica", PriceAtReservation = 6m, Quantity = 1 },
            new ReservationAddon { ReservationId = 4, AddonId = 1, AddonName = "GPS", PriceAtReservation = 4m, Quantity = 4 });

        modelBuilder.Entity<ServiceRecord>().HasData(
            new ServiceRecord { Id = 1, VehicleId = 1, ServiceDate = now.Date.AddDays(-40), Status = ServiceStatus.Completed, Description = "Mali servis", MileageAtService = 73000, Cost = 240m, NextRecommendedServiceDate = now.Date.AddDays(60) },
            new ServiceRecord { Id = 2, VehicleId = 2, ServiceDate = now.Date.AddDays(-22), Status = ServiceStatus.Completed, Description = "Zamjena guma", MileageAtService = 90000, Cost = 300m, NextRecommendedServiceDate = now.Date.AddDays(80) },
            new ServiceRecord { Id = 3, VehicleId = 3, ServiceDate = now.Date.AddDays(-15), Status = ServiceStatus.InProgress, Description = "Kocnice", MileageAtService = 125500, Cost = 410m, NextRecommendedServiceDate = now.Date.AddDays(30) },
            new ServiceRecord { Id = 4, VehicleId = 4, ServiceDate = now.Date.AddDays(-5), Status = ServiceStatus.Completed, Description = "Pregled skutera", MileageAtService = 8100, Cost = 90m, NextRecommendedServiceDate = now.Date.AddDays(120) },
            new ServiceRecord { Id = 5, VehicleId = 5, ServiceDate = now.Date.AddDays(10), Status = ServiceStatus.Planned, Description = "Lanac i ulje", MileageAtService = 14500, Cost = 180m, NextRecommendedServiceDate = now.Date.AddDays(190) });

        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FirstName = "Petra", LastName = "Barišić", JobTitle = "Voditelj poslovnice", BranchOfficeId = 1, HiredAt = now.AddYears(-4) },
            new Employee { Id = 2, FirstName = "Ivan", LastName = "Matić", JobTitle = "Dispečer voznog parka", BranchOfficeId = 2, HiredAt = now.AddYears(-2) },
            new Employee { Id = 3, FirstName = "Lucija", LastName = "Rajić", JobTitle = "Podrška korisnicima", BranchOfficeId = 3, HiredAt = now.AddMonths(-15) });

        modelBuilder.Entity<Partner>().HasData(
            new Partner { Id = 1, CompanyName = "Hotel Aurora", ContactPerson = "Mario Lovrić", Phone = "+38599888111", Email = "mario@aurora.hr" },
            new Partner { Id = 2, CompanyName = "Travel Hub Adriatic", ContactPerson = "Nina Grgić", Phone = "+38599888222", Email = "nina@travelhub.hr" },
            new Partner { Id = 3, CompanyName = "Airport Shuttle Plus", ContactPerson = "Filip Kranjčec", Phone = "+38599888333", Email = "filip@shuttleplus.hr" });
    }
}
