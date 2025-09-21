using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Grade", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 12, 3, 32, 48, 450, DateTimeKind.Utc).AddTicks(3486), null, "$2a$11$bba9cDtT2G1mgXrVBlFqle5Mfte5VFBIHPRmUNyg13UarIoh7/qS6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 12, 3, 9, 29, 239, DateTimeKind.Utc).AddTicks(3058), "$2a$11$T4wI.allyDSohxQZ5PHLTeGK584Nqns51jbiUXNbfF4ZVxH/wfTZ." });
        }
    }
}
