using CarRent.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Web.Middleware;

public sealed class PendingRoleMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPrefixes =
    [
        "/Identity/Account/Login",
        "/Identity/Account/Logout",
        "/Identity/Account/ExternalLogin",
        "/Identity/Account/PendingAccess",
        "/ClientChat",
        "/asistent",
        "/css/",
        "/js/",
        "/lib/",
        "/uploads/",
        "/favicon"
    ];

    public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (!IsAllowed(path))
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is not null)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    if (roles.Count == 0 &&
                        !path.StartsWith("/Identity/Account/PendingAccess", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.Redirect("/Identity/Account/PendingAccess");
                        return;
                    }
                }
            }
        }

        await next(context);
    }

    private static bool IsAllowed(string path) =>
        AllowedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
