using CarRent.Model.Entities;
using CarRent.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.DAL;

public class CarRentDbContext : DbContext
{
    public CarRentDbContext(DbContextOptions<CarRentDbContext> options) : base(options)
    {
    }

    public DbSet<BranchOffice> BranchOffices => Set<BranchOffice>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<ReservationAddon> ReservationAddons => Set<ReservationAddon>();
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReservationAddon>()
            .HasKey(x => new { x.ReservationId, x.AddonId });

        SeedData.Apply(modelBuilder);
    }
}
