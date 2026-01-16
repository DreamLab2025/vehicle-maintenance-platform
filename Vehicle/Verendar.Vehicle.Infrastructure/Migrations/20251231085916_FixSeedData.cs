using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xe máy hai bánh, bao gồm xe số, xe tay ga, xe côn tay" });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xe ô tô 4 bánh trở lên, bao gồm sedan, SUV, hatchback, MPV" });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "Description", "Name" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xe chạy bằng động cơ điện, bao gồm xe máy điện và ô tô điện", "Xe điện" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), "Xe máy hai bánh, bao g?m xe s?, xe tay ga, xe côn tay" });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "Description" },
                values: new object[] { new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), "Xe ô tô 4 bánh tr? lên, bao g?m sedan, SUV, hatchback, MPV" });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "Description", "Name" },
                values: new object[] { new DateTime(2025, 12, 31, 8, 17, 44, 754, DateTimeKind.Utc).AddTicks(2796), "Xe ch?y b?ng ??ng c? ?i?n, bao g?m xe máy ?i?n và ô tô ?i?n", "Xe ?i?n" });
        }
    }
}
