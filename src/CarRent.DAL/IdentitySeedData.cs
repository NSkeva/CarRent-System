using CarRent.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarRent.DAL;

public static class IdentitySeedData
{
    private static readonly string[] Roles = ["Admin", "Manager"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("CarRent.IdentitySeed");

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, logger,
            email: "admin@carrent.local",
            password: "Admin123!",
            oib: "12345678901",
            jmbg: "1234567890123",
            role: "Admin");

        await EnsureUserAsync(userManager, logger,
            email: "manager@carrent.local",
            password: "Manager123!",
            oib: "10987654321",
            jmbg: "9876543210987",
            role: "Manager");
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager,
        ILogger logger,
        string email,
        string password,
        string oib,
        string jmbg,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
            return;
        }

        user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            OIB = oib,
            JMBG = jmbg
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Seed korisnika {Email} nije uspio: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Seed korisnik {Email} ({Role}) kreiran.", email, role);
    }
}
