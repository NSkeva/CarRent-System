using CarRent.Console.Models.Enums;

namespace CarRent.Console.Models.Entities;

public class BranchOffice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<Vehicle> Vehicles { get; set; } = new();
}

public class Vehicle
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public VehicleType Type { get; set; }
    public int MileageKm { get; set; }
    public bool IsActive { get; set; }
    public decimal DailyPrice { get; set; }
    public int BranchOfficeId { get; set; }
    public List<ServiceRecord> ServiceRecords { get; set; } = new();
}

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string DriverLicenseNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Reservation> Reservations { get; set; } = new();
}

public class Reservation
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LocationType PickupLocation { get; set; }
    public LocationType DropoffLocation { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReservationAddon> Addons { get; set; } = new();

    public int GetDurationDays() => Math.Max(1, (EndDate.Date - StartDate.Date).Days);

    public decimal TotalCost() => BasePrice + Addons.Sum(a => a.PriceAtReservation * a.Quantity);
}

public class Addon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
}

public class ReservationAddon
{
    public int ReservationId { get; set; }
    public int AddonId { get; set; }
    public string AddonName { get; set; } = string.Empty;
    public decimal PriceAtReservation { get; set; }
    public int Quantity { get; set; }
}

public class ServiceRecord
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public ServiceStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal Cost { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int BranchOfficeId { get; set; }
    public DateTime HiredAt { get; set; }
}
