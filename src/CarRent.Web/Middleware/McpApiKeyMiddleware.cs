using System.Security.Claims;

namespace CarRent.Web.Middleware;

/// <summary>
/// Omogućuje MCP agentu pristup API-ju preko X-Mcp-Key headera (konfiguracija Mcp:ApiKey).
/// </summary>
public sealed class McpApiKeyMiddleware(RequestDelegate next, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var expected = config["Mcp:ApiKey"];
        var provided = context.Request.Headers["X-Mcp-Key"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(expected) &&
            string.Equals(expected, provided, StringComparison.Ordinal) &&
            context.User.Identity?.IsAuthenticated != true)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "mcp-agent"),
                new Claim(ClaimTypes.Email, "mcp@carrent.local"),
                new Claim(ClaimTypes.Role, "Admin")
            ], "McpKey");
            context.User = new ClaimsPrincipal(identity);
        }

        await next(context);
    }
}
