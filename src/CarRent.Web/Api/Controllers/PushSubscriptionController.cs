using CarRent.Web.Services;
using CarRent.Web.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CarRent.Web.Api.Controllers;

[Route("api/push")]
[ApiController]
[Authorize(Roles = "Admin,Manager")]
public sealed class PushSubscriptionController(
    PushSubscriptionService subscriptionService,
    IOptions<FleetNotificationOptions> options) : ControllerBase
{
    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey()
    {
        var key = options.Value.WebPush.PublicKey;
        if (string.IsNullOrWhiteSpace(key))
            return NotFound(new { error = "Web Push nije konfiguriran." });

        return Ok(new { publicKey = key });
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeDto dto, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        await subscriptionService.SaveAsync(userId, dto, ct);
        return Ok(new { success = true });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushUnsubscribeDto dto, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        await subscriptionService.RemoveAsync(userId, dto.Endpoint, ct);
        return Ok(new { success = true });
    }
}

public sealed class PushSubscribeDto
{
    public string Endpoint { get; set; } = string.Empty;
    public PushKeysDto Keys { get; set; } = new();
}

public sealed class PushKeysDto
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public sealed class PushUnsubscribeDto
{
    public string Endpoint { get; set; } = string.Empty;
}
