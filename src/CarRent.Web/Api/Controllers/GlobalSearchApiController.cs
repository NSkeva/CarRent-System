using CarRent.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Web.Api.Controllers;

[Route("api/search")]
[ApiController]
[Authorize]
public sealed class GlobalSearchApiController(GlobalSearchService search) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? q, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var results = await search.SearchAsync(q, isAdmin, ct);
        return Ok(results);
    }
}
