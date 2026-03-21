using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceReminders_VehiclePartTrackings_VehiclePartTracki~",
                table: "MaintenanceReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehicleBrandId",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "MaintenanceReminders");

            migrationBuilder.RenameColumn(
                name: "VehiclePartTrackingId",
                table: "MaintenanceReminders",
                newName: "TrackingCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceReminders_VehiclePartTrackingId_Level",
                table: "MaintenanceReminders",
                newName: "IX_MaintenanceReminders_TrackingCycleId_Level");

            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "VehicleModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MaintenanceReminders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TrackingCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartTrackingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartOdometer = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetOdometer = table.Column<int>(type: "integer", nullable: true),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingCycles_VehiclePartTrackings_PartTrackingId",
                        column: x => x.PartTrackingId,
                        principalTable: "VehiclePartTrackings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandId",
                table: "VehicleModels",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingCycles_PartTrackingId",
                table: "TrackingCycles",
                column: "PartTrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceReminders_TrackingCycles_TrackingCycleId",
                table: "MaintenanceReminders",
                column: "TrackingCycleId",
                principalTable: "TrackingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandId",
                table: "VehicleModels",
                column: "BrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceReminders_TrackingCycles_TrackingCycleId",
                table: "MaintenanceReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandId",
                table: "VehicleModels");

            migrationBuilder.DropTable(
                name: "TrackingCycles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_BrandId",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceReminders");

            migrationBuilder.RenameColumn(
                name: "TrackingCycleId",
                table: "MaintenanceReminders",
                newName: "VehiclePartTrackingId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceReminders_TrackingCycleId_Level",
                table: "MaintenanceReminders",
                newName: "IX_MaintenanceReminders_VehiclePartTrackingId_Level");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "MaintenanceReminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceReminders_VehiclePartTrackings_VehiclePartTracki~",
                table: "MaintenanceReminders",
                column: "VehiclePartTrackingId",
                principalTable: "VehiclePartTrackings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehicleBrandId",
                table: "VehicleModels",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
