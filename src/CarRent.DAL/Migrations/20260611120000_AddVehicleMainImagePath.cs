using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.DAL.Migrations
{
    public partial class AddVehicleMainImagePath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImagePath",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImagePath",
                table: "Vehicles");
        }
    }
}
