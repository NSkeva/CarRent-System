using System.Globalization;
using CarRent.DAL;
using CarRent.Web.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var useSqlServer = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

// SQLite: datoteka u src/CarRent.Web/Data/ — ne ovisi o tome iz kojeg foldera zoves dotnet run
static string ResolveSqlitePath(WebApplicationBuilder b)
{
    var fileName = b.Environment.IsDevelopment() ? "carrent.dev.db" : "carrent.db";
    var dataDir = Path.Combine(b.Environment.ContentRootPath, "Data");
    Directory.CreateDirectory(dataDir);
    return Path.Combine(dataDir, fileName);
}

var connectionString = useSqlServer
    ? builder.Configuration.GetConnectionString("CarRentDbSqlServer")
    : $"Data Source={ResolveSqlitePath(builder)}";

builder.Services.AddDbContext<CarRentDbContext>(options =>
{
    if (useSqlServer)
        options.UseSqlServer(connectionString, opt => opt.MigrationsAssembly("CarRent.DAL"));
    else
        options.UseSqlite(connectionString, opt => opt.MigrationsAssembly("CarRent.DAL"));
});

builder.Services.AddScoped<BranchOfficeRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<ReservationRepository>();
builder.Services.AddScoped<AddonRepository>();
builder.Services.AddScoped<ServiceRecordRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<PartnerRepository>();
builder.Services.AddScoped<DashboardRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CarRent.DbStartup");
    db.Database.Migrate();
    logger.LogInformation(
        "Baza spremna ({Provider}). Migracije primijenjene. Put: {Database}",
        useSqlServer ? "SQL Server" : "SQLite",
        useSqlServer ? connectionString : ResolveSqlitePath(builder));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[] { new CultureInfo("hr"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute("fleet_short", "vozni-park", new { controller = "Fleet", action = "Index" });
app.MapControllerRoute("daily_plan_short", "dnevni-plan", new { controller = "DailyPlan", action = "Index" });
app.MapControllerRoute("timeline_short", "raspored", new { controller = "Timeline", action = "Index" });
app.MapControllerRoute("home_hr", "pocetna", new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
