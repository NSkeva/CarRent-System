using CarRent.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Controllers;

[Route("api/lookup")]
[Authorize]
public sealed class LookupApiController(
    CustomerRepository customers,
    VehicleRepository vehicles,
    BranchOfficeRepository branches) : Controller
{
    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery] string? q)
        => Json(await customers.SearchLookupAsync(q));

    [HttpGet("vehicles")]
    public async Task<IActionResult> Vehicles([FromQuery] string? q)
        => Json(await vehicles.SearchLookupAsync(q));

    [HttpGet("branches")]
    public async Task<IActionResult> Branches([FromQuery] string? q)
        => Json(await branches.SearchLookupAsync(q));
}
