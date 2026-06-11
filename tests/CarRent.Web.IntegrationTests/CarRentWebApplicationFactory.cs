using CarRent.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CarRent.Web.IntegrationTests;

public sealed class CarRentWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"CarRentTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:CarRentDb"] = "Data Source=:memory:",
                ["Authentication:Google:ClientId"] = "test-client-id",
                ["Authentication:Google:ClientSecret"] = "test-client-secret"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            foreach (var descriptor in services.Where(d =>
                         d.ServiceType == typeof(DbContextOptions<CarRentDbContext>) ||
                         d.ServiceType == typeof(CarRentDbContext) ||
                         d.ServiceType.FullName?.Contains("DbContextOptionsConfiguration", StringComparison.Ordinal) == true)
                     .ToList())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<CarRentDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        db.Database.EnsureCreated();
        return host;
    }
}
