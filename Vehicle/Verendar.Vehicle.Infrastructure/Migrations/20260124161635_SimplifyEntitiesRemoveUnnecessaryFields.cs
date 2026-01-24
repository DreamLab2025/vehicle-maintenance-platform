using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyEntitiesRemoveUnnecessaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleTypeBrands");

            migrationBuilder.DropColumn(
                name: "IsCustomConfigured",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "EngineNumber",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "PartProducts");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "PartProducts");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "PartProducts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "GarageAddress",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "TechnicianName",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MaintenanceRecordItems");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "MaintenanceRecordItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "DefaultMaintenanceSchedules");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "MaintenanceRecordItems",
                newName: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "MaintenanceRecordItems",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomConfigured",
                table: "VehiclePartTrackings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "VehiclePartTrackings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "UserVehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserVehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "PartProducts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "PartProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "PartProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "MaintenanceRecords",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GarageAddress",
                table: "MaintenanceRecords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianName",
                table: "MaintenanceRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "MaintenanceRecordItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "MaintenanceRecordItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "DefaultMaintenanceSchedules",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VehicleTypeBrands",
                columns: table => new
                {
                    VehicleTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleBrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypeBrands", x => new { x.VehicleTypeId, x.VehicleBrandId });
                    table.ForeignKey(
                        name: "FK_VehicleTypeBrands_VehicleBrands_VehicleBrandId",
                        column: x => x.VehicleBrandId,
                        principalTable: "VehicleBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleTypeBrands_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000001"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000002"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000003"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000004"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000005-0000-0000-0000-000000000005"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000006-0000-0000-0000-000000000006"),
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "DefaultMaintenanceSchedules",
                keyColumn: "Id",
                keyValue: new Guid("d0000007-0000-0000-0000-000000000007"),
                column: "Notes",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypeBrands_VehicleBrandId",
                table: "VehicleTypeBrands",
                column: "VehicleBrandId");
        }
    }
}
