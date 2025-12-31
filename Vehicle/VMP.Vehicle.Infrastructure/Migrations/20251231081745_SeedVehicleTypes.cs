using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VMP.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedVehicleTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "VehicleTypes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "VehicleTypes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "Name", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe máy hai bánh, bao g?m xe s?, xe tay ga, xe côn tay", "Xe máy", 1, null, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe ô tô 4 bánh tr? lên, bao g?m sedan, SUV, hatchback, MPV", "Xe ô tô", 1, null, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe ch?y b?ng ??ng c? ?i?n, bao g?m xe máy ?i?n và ô tô ?i?n", "Xe ?i?n", 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DropColumn(
                name: "Description",
                table: "VehicleTypes");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "VehicleTypes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
