using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarRent.Model.Enums;

namespace CarRent.Model.Entities;

public class BranchOffice
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public LocationType LocationType { get; set; }

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Vehicle
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }
    public VehicleType Type { get; set; }
    public int MileageKm { get; set; }
    public bool IsActive { get; set; }
    public decimal DailyPrice { get; set; }

    [ForeignKey(nameof(BranchOffice))]
    public int BranchOfficeId { get; set; }

    public virtual BranchOffice? BranchOffice { get; set; }
    public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class Customer
{
    [Key]
    public int Id { get; set; }

    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(30)]
    public string DriverLicenseNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Customer))]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LocationType PickupLocation { get; set; }
    public LocationType DropoffLocation { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public virtual ICollection<ReservationAddon> Addons { get; set; } = new List<ReservationAddon>();

    public int GetDurationDays() => Math.Max(1, (EndDate.Date - StartDate.Date).Days);

    public decimal TotalCost() => BasePrice + Addons.Sum(a => a.PriceAtReservation * a.Quantity);
}

public class Addon
{
    [Key]
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public decimal PricePerDay { get; set; }

    public virtual ICollection<ReservationAddon> ReservationAddons { get; set; } = new List<ReservationAddon>();
}

public class ReservationAddon
{
    [ForeignKey(nameof(Reservation))]
    public int ReservationId { get; set; }

    [ForeignKey(nameof(Addon))]
    public int AddonId { get; set; }

    [MaxLength(80)]
    public string AddonName { get; set; } = string.Empty;

    public decimal PriceAtReservation { get; set; }
    public int Quantity { get; set; }

    public virtual Reservation? Reservation { get; set; }
    public virtual Addon? Addon { get; set; }
}

public class ServiceRecord
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }

    public DateTime ServiceDate { get; set; }
    public ServiceStatus Status { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public int MileageAtService { get; set; }
    public decimal Cost { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}

public class Employee
{
    [Key]
    public int Id { get; set; }

    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [ForeignKey(nameof(BranchOffice))]
    public int BranchOfficeId { get; set; }

    public DateTime HiredAt { get; set; }

    public virtual BranchOffice? BranchOffice { get; set; }
}

public class Partner
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string ContactPerson { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;
}
