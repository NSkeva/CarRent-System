using System.Globalization;
using CarRent.DAL;
using CarRent.Model.Entities;
using CarRent.Web.Middleware;
using CarRent.Web.Repositories;
using CarRent.Web.Services;
using CarRent.Web.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logDirectory, "carrent-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14));

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var useSqlServer = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

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

builder.Services
    .AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CarRentDbContext>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.AddScoped<BranchOfficeRepository>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<ReservationRepository>();
builder.Services.AddScoped<AddonRepository>();
builder.Services.AddScoped<ServiceRecordRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<PartnerRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.Configure<FleetLifecycleOptions>(
    builder.Configuration.GetSection(FleetLifecycleOptions.SectionName));
builder.Services.Configure<FleetNotificationOptions>(
    builder.Configuration.GetSection(FleetNotificationOptions.SectionName));
builder.Services.AddScoped<IFleetNotificationSender, PreparedFleetNotificationSender>();
builder.Services.AddScoped<FleetNotificationService>();
builder.Services.AddScoped<FleetNotificationDispatchService>();
builder.Services.AddSingleton<IEmailTransport, SmtpEmailTransport>();
builder.Services.AddSingleton<IPushTransport, WebPushTransport>();
builder.Services.AddScoped<PushSubscriptionService>();
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddHostedService<FleetNotificationOutboxDispatcher>();
builder.Services.AddScoped<ReservationSchedulingValidator>();
builder.Services.AddScoped<FleetLifecycleService>();
builder.Services.AddScoped<GlobalSearchService>();
builder.Services.AddScoped<FleetAiAvailabilityService>();
builder.Services.AddScoped<FleetClientChatConversation>();
builder.Services.AddScoped<FleetClientReservationSubmissionService>();
builder.Services.AddScoped<FleetClientChatSessionStore>();
builder.Services.AddScoped<AiClientChatService>();
builder.Services.AddScoped<AiOperatorChatService>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CarRent.DbStartup");
    db.Database.Migrate();
    logger.LogInformation(
        "Baza spremna ({Provider}). Migracije primijenjene. Put: {Database}",
        useSqlServer ? "SQL Server" : "SQLite",
        useSqlServer ? connectionString : ResolveSqlitePath(builder));
    await IdentitySeedData.SeedAsync(scope.ServiceProvider);
    await VehicleDefaultImageBootstrap.ApplyAsync(db, logger);

    var lifecycle = scope.ServiceProvider.GetRequiredService<FleetLifecycleService>();
    try
    {
        await lifecycle.SyncAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fleet lifecycle sync pri startu nije uspio — aplikacija se ipak pokreće.");
    }
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

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseMiddleware<McpApiKeyMiddleware>();
app.UseAuthorization();
app.UseMiddleware<PendingRoleMiddleware>();
app.UseMiddleware<FleetLifecycleMiddleware>();

app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute("fleet_short", "vozni-park", new { controller = "Fleet", action = "Index" });
app.MapControllerRoute("daily_plan_short", "dnevni-plan", new { controller = "DailyPlan", action = "Index" });
app.MapControllerRoute("timeline_short", "raspored", new { controller = "Timeline", action = "Index" });
app.MapControllerRoute("home_hr", "pocetna", new { controller = "Home", action = "Index" });
app.MapControllerRoute("public_assistant", "asistent", new { controller = "PublicAssistant", action = "Index" });
app.MapControllerRoute("operator_ai", "operativa/ai-asistent", new { controller = "OperatorAi", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
