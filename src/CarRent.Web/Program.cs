using CarRent.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<SeedSnapshotRepository>();
builder.Services.AddSingleton<BranchOfficeRepository>();
builder.Services.AddSingleton<VehicleRepository>();
builder.Services.AddSingleton<CustomerRepository>();
builder.Services.AddSingleton<ReservationRepository>();
builder.Services.AddSingleton<AddonRepository>();
builder.Services.AddSingleton<ServiceRecordRepository>();
builder.Services.AddSingleton<EmployeeRepository>();
builder.Services.AddSingleton<PartnerRepository>();
builder.Services.AddSingleton<DashboardRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
