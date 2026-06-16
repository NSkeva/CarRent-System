using CarRent.DAL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.DAL.Migrations;

[DbContext(typeof(CarRentDbContext))]
[Migration("20260611123000_AddVehicleRegistrationDueDate")]
partial class AddVehicleRegistrationDueDate
{
}
