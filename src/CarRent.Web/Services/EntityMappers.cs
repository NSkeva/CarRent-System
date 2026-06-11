using CarRent.Model.Entities;
using CarRent.Web.ViewModels;

namespace CarRent.Web.Services;

public static class EntityMappers
{
    public static BranchOfficeFormVm ToForm(BranchOffice e) => new()
    {
        Id = e.Id, Name = e.Name, LocationType = e.LocationType, Address = e.Address, Phone = e.Phone
    };

    public static void Apply(BranchOfficeFormVm f, BranchOffice e)
    {
        e.Name = f.Name; e.LocationType = f.LocationType; e.Address = f.Address; e.Phone = f.Phone;
    }

    public static VehicleFormVm ToForm(Vehicle e, string? branchName = null) => new()
    {
        Id = e.Id, RegistrationNumber = e.RegistrationNumber, Brand = e.Brand, Model = e.Model, Year = e.Year,
        Type = e.Type, MileageKm = e.MileageKm, IsActive = e.IsActive, DailyPrice = e.DailyPrice,
        RegistrationDueDate = e.RegistrationDueDate,
        BranchOfficeId = e.BranchOfficeId, BranchOfficeDisplay = branchName ?? string.Empty
    };

    public static void Apply(VehicleFormVm f, Vehicle e)
    {
        e.RegistrationNumber = f.RegistrationNumber; e.Brand = f.Brand; e.Model = f.Model; e.Year = f.Year;
        e.Type = f.Type; e.MileageKm = f.MileageKm; e.IsActive = f.IsActive; e.DailyPrice = f.DailyPrice;
        e.RegistrationDueDate = f.RegistrationDueDate;
        e.BranchOfficeId = f.BranchOfficeId;
    }

    public static CustomerFormVm ToForm(Customer e) => new()
    {
        Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, Email = e.Email, Phone = e.Phone,
        DateOfBirth = e.DateOfBirth, DriverLicenseNumber = e.DriverLicenseNumber
    };

    public static void Apply(CustomerFormVm f, Customer e)
    {
        e.FirstName = f.FirstName; e.LastName = f.LastName; e.Email = f.Email; e.Phone = f.Phone;
        e.DateOfBirth = f.DateOfBirth; e.DriverLicenseNumber = f.DriverLicenseNumber;
    }

    public static ReservationFormVm ToForm(Reservation e) => new()
    {
        Id = e.Id, CustomerId = e.CustomerId, VehicleId = e.VehicleId,
        CustomerDisplay = e.Customer is null ? string.Empty : $"{e.Customer.FirstName} {e.Customer.LastName}",
        VehicleDisplay = e.Vehicle is null ? string.Empty : $"{e.Vehicle.Brand} {e.Vehicle.Model}",
        StartDate = e.StartDate, EndDate = e.EndDate, PickupLocation = e.PickupLocation,
        DropoffLocation = e.DropoffLocation, Status = e.Status, BasePrice = e.BasePrice
    };

    public static void Apply(ReservationFormVm f, Reservation e)
    {
        e.CustomerId = f.CustomerId; e.VehicleId = f.VehicleId; e.StartDate = f.StartDate; e.EndDate = f.EndDate;
        e.PickupLocation = f.PickupLocation; e.DropoffLocation = f.DropoffLocation; e.Status = f.Status; e.BasePrice = f.BasePrice;
    }

    public static AddonFormVm ToForm(Addon e) => new() { Id = e.Id, Name = e.Name, PricePerDay = e.PricePerDay };
    public static void Apply(AddonFormVm f, Addon e) { e.Name = f.Name; e.PricePerDay = f.PricePerDay; }

    public static ServiceRecordFormVm ToForm(ServiceRecord e, string? vehicleLabel = null) => new()
    {
        Id = e.Id, VehicleId = e.VehicleId, VehicleDisplay = vehicleLabel ?? string.Empty,
        ServiceDate = e.ServiceDate, Status = e.Status, Description = e.Description,
        MileageAtService = e.MileageAtService, Cost = e.Cost, NextRecommendedServiceDate = e.NextRecommendedServiceDate
    };

    public static void Apply(ServiceRecordFormVm f, ServiceRecord e)
    {
        e.VehicleId = f.VehicleId; e.ServiceDate = f.ServiceDate; e.Status = f.Status; e.Description = f.Description;
        e.MileageAtService = f.MileageAtService; e.Cost = f.Cost; e.NextRecommendedServiceDate = f.NextRecommendedServiceDate;
    }

    public static EmployeeFormVm ToForm(Employee e, string? branchName = null) => new()
    {
        Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, JobTitle = e.JobTitle,
        BranchOfficeId = e.BranchOfficeId, BranchOfficeDisplay = branchName ?? string.Empty, HiredAt = e.HiredAt
    };

    public static void Apply(EmployeeFormVm f, Employee e)
    {
        e.FirstName = f.FirstName; e.LastName = f.LastName; e.JobTitle = f.JobTitle;
        e.BranchOfficeId = f.BranchOfficeId; e.HiredAt = f.HiredAt;
    }

    public static PartnerFormVm ToForm(Partner e) => new()
    {
        Id = e.Id, CompanyName = e.CompanyName, ContactPerson = e.ContactPerson, Phone = e.Phone, Email = e.Email
    };

    public static Partner ToEntity(PartnerFormVm f) => new()
    {
        Id = f.Id, CompanyName = f.CompanyName, ContactPerson = f.ContactPerson, Phone = f.Phone, Email = f.Email
    };

    public static void Apply(PartnerFormVm f, Partner e)
    {
        e.CompanyName = f.CompanyName; e.ContactPerson = f.ContactPerson; e.Phone = f.Phone; e.Email = f.Email;
    }
}
