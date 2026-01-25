using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleVariantsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VehicleVariants",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "HexCode", "ImageUrl", "UpdatedAt", "UpdatedBy", "VehicleModelId" },
                values: new object[,]
                {
                    { new Guid("e0000001-0000-0000-0000-000000000001"), "Đỏ đen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#DC143C", "", null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("e0000002-0000-0000-0000-000000000002"), "Đen bạc", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000000", "", null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("e0000003-0000-0000-0000-000000000003"), "Trắng ngọc trai", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#FFFFFF", "", null, null, new Guid("a0000002-0000-0000-0000-000000000002") },
                    { new Guid("e0000004-0000-0000-0000-000000000004"), "Xanh dương", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#4169E1", "", null, null, new Guid("a0000002-0000-0000-0000-000000000002") },
                    { new Guid("e0000005-0000-0000-0000-000000000005"), "Đen nhám", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#1C1C1C", "", null, null, new Guid("a0000003-0000-0000-0000-000000000003") },
                    { new Guid("e0000006-0000-0000-0000-000000000006"), "Đỏ GP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#FF0000", "", null, null, new Guid("a0000003-0000-0000-0000-000000000003") },
                    { new Guid("e0000007-0000-0000-0000-000000000007"), "Xanh đen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000080", "", null, null, new Guid("a0000004-0000-0000-0000-000000000004") },
                    { new Guid("e0000008-0000-0000-0000-000000000008"), "Đen nhám", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000000", "", null, null, new Guid("a0000004-0000-0000-0000-000000000004") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000008-0000-0000-0000-000000000008"));
        }
    }
}
