using System.ComponentModel.DataAnnotations;
using CarRent.Model.Enums;

namespace CarRent.Web.ViewModels;

public sealed class AutocompleteFieldVm
{
    public required string InputId { get; init; }
    public required string HiddenName { get; init; }
    public required string Label { get; init; }
    public required string SearchUrl { get; init; }
    public int? SelectedId { get; init; }
    public string DisplayText { get; init; } = string.Empty;
}

public sealed class DateTimeFieldVm
{
    public required string FieldName { get; init; }
    public required string Label { get; init; }
    public DateTime? Value { get; init; }
}

public sealed class BranchOfficeFormVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public LocationType LocationType { get; set; }

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;
}

public sealed class VehicleFormVm
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Brand { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Required]
    public VehicleType Type { get; set; }

    [Range(0, 2_000_000)]
    public int MileageKm { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(1, 10_000)]
    public decimal DailyPrice { get; set; }

    [Required]
    public int BranchOfficeId { get; set; }

    public string BranchOfficeDisplay { get; set; } = string.Empty;
}

public sealed class CustomerFormVm
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-25);

    [Required, StringLength(30)]
    public string DriverLicenseNumber { get; set; } = string.Empty;
}

public sealed class ReservationFormVm
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public string CustomerDisplay { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    public string VehicleDisplay { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(1);

    [Required]
    public LocationType PickupLocation { get; set; }

    [Required]
    public LocationType DropoffLocation { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    [Range(0, 100_000)]
    public decimal BasePrice { get; set; }
}

public sealed class AddonFormVm
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10_000)]
    public decimal PricePerDay { get; set; }
}

public sealed class ServiceRecordFormVm
{
    public int Id { get; set; }

    [Required]
    public int VehicleId { get; set; }

    public string VehicleDisplay { get; set; } = string.Empty;

    [Required]
    public DateTime ServiceDate { get; set; } = DateTime.UtcNow;

    [Required]
    public ServiceStatus Status { get; set; } = ServiceStatus.Planned;

    [Required, StringLength(300)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 2_000_000)]
    public int MileageAtService { get; set; }

    [Range(0, 100_000)]
    public decimal Cost { get; set; }

    public DateTime? NextRecommendedServiceDate { get; set; }
}

public sealed class EmployeeFormVm
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public int BranchOfficeId { get; set; }

    public string BranchOfficeDisplay { get; set; } = string.Empty;

    [Required]
    public DateTime HiredAt { get; set; } = DateTime.UtcNow;
}

public sealed class PartnerFormVm
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;
}

public sealed class LookupItemVm
{
    public int Id { get; set; }
    public required string Label { get; set; }
}
