using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Assignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 12, 3, 9, 29, 239, DateTimeKind.Utc).AddTicks(3058), "$2a$11$T4wI.allyDSohxQZ5PHLTeGK584Nqns51jbiUXNbfF4ZVxH/wfTZ." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Assignments");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 11, 16, 38, 14, 634, DateTimeKind.Utc).AddTicks(9340), "$2a$11$MNmpZXa8Z.SElWzlQ3C3LeEIhRQM8vRr64gtg4DBnobvf9TMcNG0G" });
        }
    }
}
