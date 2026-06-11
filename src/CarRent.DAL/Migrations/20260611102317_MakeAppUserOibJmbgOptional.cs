using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeAppUserOibJmbgOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OIB",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 11);

            migrationBuilder.AlterColumn<string>(
                name: "JMBG",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OIB",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 11,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JMBG",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13,
                oldNullable: true);
        }
    }
}
