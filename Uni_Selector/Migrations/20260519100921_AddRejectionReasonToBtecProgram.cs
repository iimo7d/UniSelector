using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uni_Selector.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToBtecProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "BtecPrograms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 1,
                column: "RejectionReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 2,
                column: "RejectionReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "BtecPrograms",
                keyColumn: "Id",
                keyValue: 3,
                column: "RejectionReason",
                value: null);

            migrationBuilder.InsertData(
                table: "UniversityRepresentatives",
                columns: new[] { "Id", "AssignedAt", "CanManageFees", "CanManagePrograms", "CanViewApplications", "IsActive", "Position", "UniversityId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, true, true, "Admissions Officer", 1, "rep-001" },
                    { 2, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, true, true, "Admissions Officer", 2, "rep-002" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UniversityRepresentatives",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UniversityRepresentatives",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "BtecPrograms");
        }
    }
}
