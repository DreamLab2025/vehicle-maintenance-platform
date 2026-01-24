using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedVehicleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.InsertData(
                table: "PartCategories",
                columns: new[] { "Id", "AllowsMultipleInstances", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconUrl", "Name", "RequiresOdometerTracking", "RequiresTimeTracking", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("c0000001-0000-0000-0000-000000000001"), false, "ENGINE-OIL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Dầu bôi trơn động cơ, cần thay định kỳ theo km hoặc thời gian", 1, null, "Dầu nhớt động cơ", true, true, 1, null, null },
                    { new Guid("c0000002-0000-0000-0000-000000000002"), true, "TIRE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Lốp trước và lốp sau, thay khi mòn hoặc đạt tuổi thọ", 2, null, "Lốp xe", true, true, 1, null, null },
                    { new Guid("c0000003-0000-0000-0000-000000000003"), false, "BATTERY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bình điện ắc quy, thay khi hết tuổi thọ hoặc không giữ điện", 3, null, "Ắc quy", false, true, 1, null, null },
                    { new Guid("c0000004-0000-0000-0000-000000000004"), true, "BRAKE-PAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Má phanh trước và sau, thay khi mòn", 4, null, "Má phanh", true, true, 1, null, null },
                    { new Guid("c0000005-0000-0000-0000-000000000005"), false, "SPARK-PLUG", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bugi đánh lửa động cơ, thay định kỳ", 5, null, "Bugi", true, true, 1, null, null },
                    { new Guid("c0000006-0000-0000-0000-000000000006"), false, "AIR-FILTER", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Lọc không khí động cơ, cần vệ sinh và thay định kỳ", 6, null, "Lọc gió", true, true, 1, null, null },
                    { new Guid("c0000007-0000-0000-0000-000000000007"), false, "CHAIN-SPROCKET", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bộ truyền động xích và nhông, thay khi mòn", 7, null, "Nhông sên dĩa", true, false, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "VehicleBrands",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "LogoUrl", "Name", "Status", "SupportPhone", "UpdatedAt", "UpdatedBy", "VehicleTypeId", "Website" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), "HONDA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Honda", 1, "18001076", null, null, new Guid("11111111-1111-1111-1111-111111111111"), "https://www.honda.com.vn" },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), "YAMAHA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Yamaha", 1, "18006019", null, null, new Guid("11111111-1111-1111-1111-111111111111"), "https://www.yamaha-motor.com.vn" }
                });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "MOTORCYCLE", "Xe máy" });

            migrationBuilder.InsertData(
                table: "VehicleModels",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "EngineCapacity", "EngineDisplacement", "FuelType", "ManufactureYear", "Name", "Status", "TransmissionType", "UpdatedAt", "UpdatedBy", "VehicleBrandId" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000001"), "WAVE-ALPHA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1.10m, 110, 1, 2020, "Wave Alpha", 1, 1, null, null, new Guid("b0000001-0000-0000-0000-000000000001") },
                    { new Guid("a0000002-0000-0000-0000-000000000002"), "AIR-BLADE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1.25m, 125, 1, 2021, "Air Blade", 1, 2, null, null, new Guid("b0000001-0000-0000-0000-000000000001") },
                    { new Guid("a0000003-0000-0000-0000-000000000003"), "EXCITER", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1.55m, 155, 1, 2021, "Exciter", 1, 1, null, null, new Guid("b0000002-0000-0000-0000-000000000002") },
                    { new Guid("a0000004-0000-0000-0000-000000000004"), "SIRIUS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1.10m, 110, 1, 2020, "Sirius", 1, 1, null, null, new Guid("b0000002-0000-0000-0000-000000000002") }
                });

            migrationBuilder.InsertData(
                table: "DefaultMaintenanceSchedules",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "InitialKm", "KmInterval", "MonthsInterval", "Notes", "PartCategoryId", "Status", "UpdatedAt", "UpdatedBy", "VehicleModelId" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 1000, 3000, 4, null, new Guid("c0000001-0000-0000-0000-000000000001"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 20000, 36, null, new Guid("c0000002-0000-0000-0000-000000000002"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 0, 24, null, new Guid("c0000003-0000-0000-0000-000000000003"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 10000, 12, null, new Guid("c0000004-0000-0000-0000-000000000004"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 8000, 12, null, new Guid("c0000005-0000-0000-0000-000000000005"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000006-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 6000, 6, null, new Guid("c0000006-0000-0000-0000-000000000006"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000007-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 15000, 0, null, new Guid("c0000007-0000-0000-0000-000000000007"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "VehicleModels",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "", "Motorcycle" });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "Name", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe ô tô 4 bánh trở lên, bao gồm sedan, SUV, hatchback, MPV", "Car", 1, null, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe chạy bằng động cơ điện, bao gồm xe máy điện và ô tô điện", "Electric Vehicle", 1, null, null }
                });
        }
    }
}
