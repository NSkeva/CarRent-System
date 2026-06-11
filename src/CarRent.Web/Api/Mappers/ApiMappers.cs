using CarRent.Model.Entities;
using CarRent.Web.Api.Dtos;

namespace CarRent.Web.Api.Mappers;

public static class ApiMappers
{
    public static BranchOfficeSummaryDto ToSummary(BranchOffice e) => new() { Id = e.Id, Name = e.Name };

    public static BranchOfficeDto ToDto(BranchOffice e) => new()
    {
        Id = e.Id, Name = e.Name, LocationType = e.LocationType, Address = e.Address, Phone = e.Phone
    };

    public static void Apply(BranchOfficeCreateDto dto, BranchOffice e)
    {
        e.Name = dto.Name;
        e.LocationType = dto.LocationType;
        e.Address = dto.Address;
        e.Phone = dto.Phone;
    }

    public static VehicleSummaryDto ToSummary(Vehicle e) => new()
    {
        Id = e.Id, RegistrationNumber = e.RegistrationNumber, Brand = e.Brand, Model = e.Model
    };

    public static VehicleDto ToDto(Vehicle e) => new()
    {
        Id = e.Id,
        RegistrationNumber = e.RegistrationNumber,
        Brand = e.Brand,
        Model = e.Model,
        Year = e.Year,
        Type = e.Type,
        MileageKm = e.MileageKm,
        IsActive = e.IsActive,
        DailyPrice = e.DailyPrice,
        BranchOfficeId = e.BranchOfficeId,
        BranchOffice = e.BranchOffice is null ? null : ToSummary(e.BranchOffice)
    };

    public static void Apply(VehicleCreateDto dto, Vehicle e)
    {
        e.RegistrationNumber = dto.RegistrationNumber;
        e.Brand = dto.Brand;
        e.Model = dto.Model;
        e.Year = dto.Year;
        e.Type = dto.Type;
        e.MileageKm = dto.MileageKm;
        e.IsActive = dto.IsActive;
        e.DailyPrice = dto.DailyPrice;
        e.BranchOfficeId = dto.BranchOfficeId;
    }

    public static CustomerSummaryDto ToSummary(Customer e) => new()
    {
        Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, Email = e.Email
    };

    public static CustomerDto ToDto(Customer e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        DateOfBirth = e.DateOfBirth,
        DriverLicenseNumber = e.DriverLicenseNumber,
        CreatedAt = e.CreatedAt
    };

    public static void Apply(CustomerCreateDto dto, Customer e)
    {
        e.FirstName = dto.FirstName;
        e.LastName = dto.LastName;
        e.Email = dto.Email;
        e.Phone = dto.Phone;
        e.DateOfBirth = dto.DateOfBirth;
        e.DriverLicenseNumber = dto.DriverLicenseNumber;
    }

    public static ReservationDto ToDto(Reservation e) => new()
    {
        Id = e.Id,
        CustomerId = e.CustomerId,
        VehicleId = e.VehicleId,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        PickupLocation = e.PickupLocation,
        DropoffLocation = e.DropoffLocation,
        Status = e.Status,
        BasePrice = e.BasePrice,
        CreatedAt = e.CreatedAt,
        Customer = e.Customer is null ? null : ToSummary(e.Customer),
        Vehicle = e.Vehicle is null ? null : ToSummary(e.Vehicle)
    };

    public static void Apply(ReservationCreateDto dto, Reservation e)
    {
        e.CustomerId = dto.CustomerId;
        e.VehicleId = dto.VehicleId;
        e.StartDate = dto.StartDate;
        e.EndDate = dto.EndDate;
        e.PickupLocation = dto.PickupLocation;
        e.DropoffLocation = dto.DropoffLocation;
        e.Status = dto.Status;
        e.BasePrice = dto.BasePrice;
    }

    public static AddonDto ToDto(Addon e) => new() { Id = e.Id, Name = e.Name, PricePerDay = e.PricePerDay };

    public static void Apply(AddonCreateDto dto, Addon e)
    {
        e.Name = dto.Name;
        e.PricePerDay = dto.PricePerDay;
    }

    public static ServiceRecordDto ToDto(ServiceRecord e) => new()
    {
        Id = e.Id,
        VehicleId = e.VehicleId,
        ServiceDate = e.ServiceDate,
        Status = e.Status,
        Description = e.Description,
        MileageAtService = e.MileageAtService,
        Cost = e.Cost,
        NextRecommendedServiceDate = e.NextRecommendedServiceDate,
        Vehicle = e.Vehicle is null ? null : ToSummary(e.Vehicle)
    };

    public static void Apply(ServiceRecordCreateDto dto, ServiceRecord e)
    {
        e.VehicleId = dto.VehicleId;
        e.ServiceDate = dto.ServiceDate;
        e.Status = dto.Status;
        e.Description = dto.Description;
        e.MileageAtService = dto.MileageAtService;
        e.Cost = dto.Cost;
        e.NextRecommendedServiceDate = dto.NextRecommendedServiceDate;
    }

    public static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        JobTitle = e.JobTitle,
        BranchOfficeId = e.BranchOfficeId,
        HiredAt = e.HiredAt,
        BranchOffice = e.BranchOffice is null ? null : ToSummary(e.BranchOffice)
    };

    public static void Apply(EmployeeCreateDto dto, Employee e)
    {
        e.FirstName = dto.FirstName;
        e.LastName = dto.LastName;
        e.JobTitle = dto.JobTitle;
        e.BranchOfficeId = dto.BranchOfficeId;
        e.HiredAt = dto.HiredAt;
    }

    public static PartnerDto ToDto(Partner e) => new()
    {
        Id = e.Id,
        CompanyName = e.CompanyName,
        ContactPerson = e.ContactPerson,
        Phone = e.Phone,
        Email = e.Email
    };

    public static void Apply(PartnerCreateDto dto, Partner e)
    {
        e.CompanyName = dto.CompanyName;
        e.ContactPerson = dto.ContactPerson;
        e.Phone = dto.Phone;
        e.Email = dto.Email;
    }
}
