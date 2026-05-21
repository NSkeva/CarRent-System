using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarRent.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PricePerDay = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BranchOffices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    LocationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchOffices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DriverLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BranchOfficeId = table.Column<int>(type: "INTEGER", nullable: false),
                    HiredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_BranchOffices_BranchOfficeId",
                        column: x => x.BranchOfficeId,
                        principalTable: "BranchOffices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MileageKm = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    BranchOfficeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_BranchOffices_BranchOfficeId",
                        column: x => x.BranchOfficeId,
                        principalTable: "BranchOffices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PickupLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    DropoffLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BasePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    MileageAtService = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    NextRecommendedServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservationAddons",
                columns: table => new
                {
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddonName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PriceAtReservation = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationAddons", x => new { x.ReservationId, x.AddonId });
                    table.ForeignKey(
                        name: "FK_ReservationAddons_Addons_AddonId",
                        column: x => x.AddonId,
                        principalTable: "Addons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationAddons_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Addons",
                columns: new[] { "Id", "Name", "PricePerDay" },
                values: new object[,]
                {
                    { 1, "GPS", 4m },
                    { 2, "Djecja sjedalica", 6m },
                    { 3, "Premium osiguranje", 12m }
                });

            migrationBuilder.InsertData(
                table: "BranchOffices",
                columns: new[] { "Id", "Address", "LocationType", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "Ilica 1", 2, "Zagreb Centar", "+385111111" },
                    { 2, "Airport Rd 10", 1, "Zagreb Airport", "+385222222" },
                    { 3, "Riva 5", 0, "Split Riva", "+385333333" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "DriverLicenseNumber", "Email", "FirstName", "LastName", "Phone" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 2, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1994, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR001", "marko@example.com", "Marko", "Horvat", "+38591000111" },
                    { 2, new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1998, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR002", "iva@example.com", "Iva", "Peric", "+38591000222" },
                    { 3, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1989, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR003", "luka@example.com", "Luka", "Maric", "+38591000333" },
                    { 4, new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR004", "ana@example.com", "Ana", "Kovacic", "+38591000444" }
                });

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[] { "Id", "CompanyName", "ContactPerson", "Email", "Phone" },
                values: new object[,]
                {
                    { 1, "Hotel Aurora", "Mario Lovrić", "mario@aurora.hr", "+38599888111" },
                    { 2, "Travel Hub Adriatic", "Nina Grgić", "nina@travelhub.hr", "+38599888222" },
                    { 3, "Airport Shuttle Plus", "Filip Kranjčec", "filip@shuttleplus.hr", "+38599888333" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "BranchOfficeId", "FirstName", "HiredAt", "JobTitle", "LastName" },
                values: new object[,]
                {
                    { 1, 1, "Petra", new DateTime(2022, 4, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Voditelj poslovnice", "Barišić" },
                    { 2, 2, "Ivan", new DateTime(2024, 4, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Dispečer voznog parka", "Matić" },
                    { 3, 3, "Lucija", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Podrška korisnicima", "Rajić" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "BranchOfficeId", "Brand", "DailyPrice", "IsActive", "MileageKm", "Model", "RegistrationNumber", "Type", "Year" },
                values: new object[,]
                {
                    { 1, 1, "Skoda", 55m, true, 75500, "Octavia", "ZG-100-AA", 0, 2021 },
                    { 2, 1, "Volkswagen", 50m, true, 91400, "Golf", "ZG-200-BB", 0, 2020 },
                    { 3, 2, "Renault", 79m, true, 126000, "Trafic", "ZG-300-CC", 1, 2019 },
                    { 4, 3, "Piaggio", 30m, true, 8300, "Liberty", "ST-400-DD", 2, 2023 },
                    { 5, 3, "Yamaha", 64m, true, 14500, "MT-07", "ST-500-EE", 3, 2022 },
                    { 6, 2, "Cube", 18m, true, 1900, "Touring", "ZG-600-FF", 4, 2024 }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BasePrice", "CreatedAt", "CustomerId", "DropoffLocation", "EndDate", "PickupLocation", "StartDate", "Status", "VehicleId" },
                values: new object[,]
                {
                    { 1, 165m, new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1 },
                    { 2, 316m, new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 3, 30m, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, new DateTime(2026, 4, 17, 0, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2026, 4, 16, 0, 0, 0, 0, DateTimeKind.Utc), 2, 4 },
                    { 4, 200m, new DateTime(2026, 4, 15, 4, 0, 0, 0, DateTimeKind.Utc), 4, 2, new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2026, 4, 21, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 },
                    { 5, 256m, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Utc), 3, 5 },
                    { 6, 36m, new DateTime(2026, 4, 15, 16, 0, 0, 0, DateTimeKind.Utc), 2, 2, new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2026, 4, 17, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6 }
                });

            migrationBuilder.InsertData(
                table: "ServiceRecords",
                columns: new[] { "Id", "Cost", "Description", "MileageAtService", "NextRecommendedServiceDate", "ServiceDate", "Status", "VehicleId" },
                values: new object[,]
                {
                    { 1, 240m, "Mali servis", 73000, new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1 },
                    { 2, 300m, "Zamjena guma", 90000, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2 },
                    { 3, 410m, "Kocnice", 125500, new DateTime(2026, 5, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 4, 90m, "Pregled skutera", 8100, new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 4 },
                    { 5, 180m, "Lanac i ulje", 14500, new DateTime(2026, 10, 23, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), 0, 5 }
                });

            migrationBuilder.InsertData(
                table: "ReservationAddons",
                columns: new[] { "AddonId", "ReservationId", "AddonName", "PriceAtReservation", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, "GPS", 4m, 3 },
                    { 3, 2, "Premium osiguranje", 12m, 4 },
                    { 2, 3, "Djecja sjedalica", 6m, 1 },
                    { 1, 4, "GPS", 4m, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchOfficeId",
                table: "Employees",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationAddons_AddonId",
                table: "ReservationAddons",
                column: "AddonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VehicleId",
                table: "Reservations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_VehicleId",
                table: "ServiceRecords",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_BranchOfficeId",
                table: "Vehicles",
                column: "BranchOfficeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "ReservationAddons");

            migrationBuilder.DropTable(
                name: "ServiceRecords");

            migrationBuilder.DropTable(
                name: "Addons");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "BranchOffices");
        }
    }
}
