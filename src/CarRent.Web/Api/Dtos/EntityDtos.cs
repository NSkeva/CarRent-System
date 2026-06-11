using System.ComponentModel.DataAnnotations;
using CarRent.Model.Enums;

namespace CarRent.Web.Api.Dtos;

public sealed class BranchOfficeSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class VehicleSummaryDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

public sealed class CustomerSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class BranchOfficeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class BranchOfficeCreateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public LocationType LocationType { get; set; }

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;
}

public sealed class BranchOfficeUpdateDto : BranchOfficeCreateDto
{
    public int Id { get; set; }
}

public sealed class VehicleDto
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
    public BranchOfficeSummaryDto? BranchOffice { get; set; }
}

public class VehicleCreateDto
{
    [Required, MaxLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Model { get; set; } = string.Empty;

    [Range(1980, 2100)]
    public int Year { get; set; }

    public VehicleType Type { get; set; }

    [Range(0, int.MaxValue)]
    public int MileageKm { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, double.MaxValue)]
    public decimal DailyPrice { get; set; }

    public int BranchOfficeId { get; set; }
}

public sealed class VehicleUpdateDto : VehicleCreateDto
{
    public int Id { get; set; }
}

public sealed class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string DriverLicenseNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CustomerCreateDto
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(30)]
    public string DriverLicenseNumber { get; set; } = string.Empty;
}

public sealed class CustomerUpdateDto : CustomerCreateDto
{
    public int Id { get; set; }
}

public sealed class ReservationDto
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
    public CustomerSummaryDto? Customer { get; set; }
    public VehicleSummaryDto? Vehicle { get; set; }
}

public class ReservationCreateDto
{
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LocationType PickupLocation { get; set; }
    public LocationType DropoffLocation { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal BasePrice { get; set; }
}

public sealed class ReservationUpdateDto : ReservationCreateDto
{
    public int Id { get; set; }
}

public sealed class AddonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
}

public class AddonCreateDto
{
    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PricePerDay { get; set; }
}

public sealed class AddonUpdateDto : AddonCreateDto
{
    public int Id { get; set; }
}

public sealed class ServiceRecordDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public ServiceStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal Cost { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
    public VehicleSummaryDto? Vehicle { get; set; }
}

public class ServiceRecordCreateDto
{
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public ServiceStatus Status { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public int MileageAtService { get; set; }
    public decimal Cost { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
}

public sealed class ServiceRecordUpdateDto : ServiceRecordCreateDto
{
    public int Id { get; set; }
}

public sealed class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int BranchOfficeId { get; set; }
    public DateTime HiredAt { get; set; }
    public BranchOfficeSummaryDto? BranchOffice { get; set; }
}

public class EmployeeCreateDto
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    public int BranchOfficeId { get; set; }
    public DateTime HiredAt { get; set; }
}

public sealed class EmployeeUpdateDto : EmployeeCreateDto
{
    public int Id { get; set; }
}

public sealed class PartnerDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class PartnerCreateDto
{
    [Required, MaxLength(120)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string ContactPerson { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;
}

public sealed class PartnerUpdateDto : PartnerCreateDto
{
    public int Id { get; set; }
}
