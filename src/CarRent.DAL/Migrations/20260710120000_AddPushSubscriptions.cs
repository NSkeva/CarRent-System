using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.DAL.Migrations
{
    public partial class AddPushSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FleetPushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    P256dh = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Auth = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FleetPushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FleetPushSubscriptions_UserId_Endpoint",
                table: "FleetPushSubscriptions",
                columns: new[] { "UserId", "Endpoint" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FleetPushSubscriptions");
        }
    }
}
