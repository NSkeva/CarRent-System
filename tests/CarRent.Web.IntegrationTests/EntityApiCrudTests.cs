using System.Net;
using System.Net.Http.Json;
using CarRent.Model.Enums;
using CarRent.Web.Api.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CarRent.Web.IntegrationTests;

public sealed class EntityApiCrudTests : IClassFixture<CarRentWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EntityApiCrudTests(CarRentWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task BranchOffice_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/branch-office");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<BranchOfficeDto>>();
        items.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public async Task BranchOffice_GetById_WhenExists_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/branch-office/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<BranchOfficeDto>();
        dto!.Id.Should().Be(1);
    }

    [Fact]
    public async Task BranchOffice_GetById_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/branch-office/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BranchOffice_Post_WhenValid_ReturnsCreated()
    {
        var client = _client.AsRole("Admin");
        var payload = new BranchOfficeCreateDto
        {
            Name = "Test poslovnica",
            LocationType = LocationType.Downtown,
            Address = "Test ulica 1",
            Phone = "+38599111222"
        };

        var response = await client.PostAsJsonAsync("/api/branch-office", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task BranchOffice_Post_WhenInvalid_ReturnsBadRequest()
    {
        var client = _client.AsRole("Admin");
        var payload = new BranchOfficeCreateDto { Name = "" };
        var response = await client.PostAsJsonAsync("/api/branch-office", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BranchOffice_GetAll_WhenUnauthorized_ReturnsUnauthorized()
    {
        var response = await _client.AsAnonymous().GetAsync("/api/branch-office");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BranchOffice_Post_WhenUnauthorized_ReturnsUnauthorized()
    {
        var client = _client.AsAnonymous();
        var payload = new BranchOfficeCreateDto { Name = "Bez auth" };
        var response = await client.PostAsJsonAsync("/api/branch-office", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BranchOffice_Put_WhenExists_ReturnsOk()
    {
        var client = _client.AsRole("Manager");
        var payload = new BranchOfficeUpdateDto
        {
            Id = 1,
            Name = "Zagreb Centar API",
            LocationType = LocationType.Downtown,
            Address = "Ilica 1",
            Phone = "+385111111"
        };
        var response = await client.PutAsJsonAsync("/api/branch-office/1", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BranchOffice_Put_WhenMissing_ReturnsNotFound()
    {
        var client = _client.AsRole("Admin");
        var payload = new BranchOfficeUpdateDto { Id = 99999, Name = "X" };
        var response = await client.PutAsJsonAsync("/api/branch-office/99999", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BranchOffice_Delete_WhenMissing_ReturnsNotFound()
    {
        var client = _client.AsRole("Admin");
        var response = await client.DeleteAsync("/api/branch-office/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Vehicle_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/vehicle");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Vehicle_GetById_WhenExists_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/vehicle/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Vehicle_GetById_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/vehicle/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Vehicle_Post_WhenValid_ReturnsCreated()
    {
        var client = _client.AsRole("Manager");
        var payload = new VehicleCreateDto
        {
            RegistrationNumber = "TST-001",
            Brand = "Test",
            Model = "Car",
            Year = 2024,
            Type = VehicleType.Car,
            MileageKm = 1000,
            DailyPrice = 40,
            BranchOfficeId = 1
        };
        var response = await client.PostAsJsonAsync("/api/vehicle", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Vehicle_Post_WhenInvalid_ReturnsBadRequest()
    {
        var client = _client.AsRole("Admin");
        var payload = new VehicleCreateDto { RegistrationNumber = "" };
        var response = await client.PostAsJsonAsync("/api/vehicle", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Vehicle_Put_WhenMissing_ReturnsNotFound()
    {
        var client = _client.AsRole("Admin");
        var payload = new VehicleUpdateDto
        {
            Id = 99999,
            RegistrationNumber = "TST-404",
            Brand = "Test",
            Model = "Car",
            Year = 2024,
            Type = VehicleType.Car,
            MileageKm = 1,
            DailyPrice = 10,
            BranchOfficeId = 1
        };
        var response = await client.PutAsJsonAsync("/api/vehicle/99999", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/customer");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Customer_GetById_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/customer/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_Post_WhenValid_ReturnsCreated()
    {
        var client = _client.AsRole("Admin");
        var payload = new CustomerCreateDto
        {
            FirstName = "Test",
            LastName = "Korisnik",
            Email = "test.user@example.com",
            Phone = "+38598111222",
            DateOfBirth = new DateTime(1995, 1, 1),
            DriverLicenseNumber = "HR999"
        };
        var response = await client.PostAsJsonAsync("/api/customer", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Customer_Post_WhenInvalid_ReturnsBadRequest()
    {
        var client = _client.AsRole("Admin");
        var payload = new CustomerCreateDto { FirstName = "", LastName = "", Email = "not-an-email" };
        var response = await client.PostAsJsonAsync("/api/customer", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reservation_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/reservation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reservation_GetById_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/reservation/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reservation_Post_WhenInvalidDates_ReturnsBadRequest()
    {
        var client = _client.AsRole("Manager");
        var payload = new ReservationCreateDto
        {
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(1),
            PickupLocation = LocationType.Downtown,
            DropoffLocation = LocationType.Airport,
            Status = ReservationStatus.Confirmed,
            BasePrice = 100
        };
        var response = await client.PostAsJsonAsync("/api/reservation", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Addon_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/addon");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Addon_Post_WhenValid_ReturnsCreated()
    {
        var client = _client.AsRole("Admin");
        var payload = new AddonCreateDto { Name = "WiFi", PricePerDay = 3 };
        var response = await client.PostAsJsonAsync("/api/addon", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Addon_Delete_WhenMissing_ReturnsNotFound()
    {
        var client = _client.AsRole("Admin");
        var response = await client.DeleteAsync("/api/addon/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ServiceRecord_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/service-record");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceRecord_Post_WhenValid_ReturnsCreated()
    {
        var client = _client.AsRole("Manager");
        var payload = new ServiceRecordCreateDto
        {
            VehicleId = 1,
            ServiceDate = DateTime.UtcNow,
            Status = ServiceStatus.Planned,
            Description = "Test servis",
            MileageAtService = 1000,
            Cost = 50
        };
        var response = await client.PostAsJsonAsync("/api/service-record", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Employee_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/employee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Employee_Put_WhenMissing_ReturnsNotFound()
    {
        var client = _client.AsRole("Admin");
        var payload = new EmployeeUpdateDto { Id = 99999, FirstName = "X", LastName = "Y", BranchOfficeId = 1 };
        var response = await client.PutAsJsonAsync("/api/employee/99999", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Partner_GetAll_ReturnsOk()
    {
        var response = await _client.AsRole("Manager").GetAsync("/api/partner");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Partner_Post_WhenUnauthorized_ReturnsUnauthorized()
    {
        var client = _client.AsAnonymous();
        var payload = new PartnerCreateDto { CompanyName = "Test d.o.o." };
        var response = await client.PostAsJsonAsync("/api/partner", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Partner_Delete_AsManager_ReturnsForbidden()
    {
        var client = _client.AsRole("Manager");
        var response = await client.DeleteAsync("/api/partner/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
