using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntityStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_VehicleBrandId_Status",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_UserId_Status",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_PartProducts_PartCategoryId_Status",
                table: "PartProducts");

            migrationBuilder.DropIndex(
                name: "IX_PartCategories_DisplayOrder_Status",
                table: "PartCategories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleBrands");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartProducts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartCategories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DefaultMaintenanceSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_VehicleBrandId",
                table: "VehicleModels",
                column: "VehicleBrandId",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_UserId",
                table: "UserVehicles",
                column: "UserId",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartProducts_PartCategoryId",
                table: "PartProducts",
                column: "PartCategoryId",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_DisplayOrder",
                table: "PartCategories",
                column: "DisplayOrder",
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_VehicleBrandId",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_UserId",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_PartProducts_PartCategoryId",
                table: "PartProducts");

            migrationBuilder.DropIndex(
                name: "IX_PartCategories_DisplayOrder",
                table: "PartCategories");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehicleTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehiclePartTrackings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehicleModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehicleBrands",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserVehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PartProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PartCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MaintenanceRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DefaultMaintenanceSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_VehicleBrandId_Status",
                table: "VehicleModels",
                columns: new[] { "VehicleBrandId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_UserId_Status",
                table: "UserVehicles",
                columns: new[] { "UserId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartProducts_PartCategoryId_Status",
                table: "PartProducts",
                columns: new[] { "PartCategoryId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_DisplayOrder_Status",
                table: "PartCategories",
                columns: new[] { "DisplayOrder", "Status" },
                filter: "\"DeletedAt\" IS NULL");
        }
    }
}
