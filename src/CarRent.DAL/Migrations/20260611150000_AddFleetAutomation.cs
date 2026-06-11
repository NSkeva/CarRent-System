using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.DAL.Migrations
{
    public partial class AddFleetAutomation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlockedByService",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveBeforeServiceBlock",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MileageUpdateSuggested",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FleetNotificationOutbox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Channel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NotificationType = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Recipient = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DedupKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FleetNotificationOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FleetNotificationOutbox_DedupKey",
                table: "FleetNotificationOutbox",
                column: "DedupKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FleetNotificationOutbox");

            migrationBuilder.DropColumn(name: "MileageUpdateSuggested", table: "Reservations");

            migrationBuilder.DropColumn(name: "IsActiveBeforeServiceBlock", table: "Vehicles");

            migrationBuilder.DropColumn(name: "BlockedByService", table: "Vehicles");
        }
    }
}
