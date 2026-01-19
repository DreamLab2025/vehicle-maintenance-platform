using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VMP.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameAndCreateNewOilTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceActivityDetails_ConsumableItems_ConsumableItemId",
                table: "MaintenanceActivityDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardMaintenanceSchedules_ConsumableItems_ConsumableItem~",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMaintenanceConfigs_ConsumableItems_ConsumableItemId",
                table: "UserMaintenanceConfigs");

            migrationBuilder.DropTable(
                name: "ConsumableItems");

            migrationBuilder.RenameColumn(
                name: "ConsumableItemId",
                table: "UserMaintenanceConfigs",
                newName: "VehiclePartId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMaintenanceConfigs_ConsumableItemId",
                table: "UserMaintenanceConfigs",
                newName: "IX_UserMaintenanceConfigs_VehiclePartId");

            migrationBuilder.RenameColumn(
                name: "ConsumableItemId",
                table: "StandardMaintenanceSchedules",
                newName: "VehiclePartId");

            migrationBuilder.RenameIndex(
                name: "IX_StandardMaintenanceSchedules_VehicleModelId_ConsumableItemId",
                table: "StandardMaintenanceSchedules",
                newName: "IX_StandardMaintenanceSchedules_VehicleModelId_VehiclePartId");

            migrationBuilder.RenameIndex(
                name: "IX_StandardMaintenanceSchedules_ConsumableItemId",
                table: "StandardMaintenanceSchedules",
                newName: "IX_StandardMaintenanceSchedules_VehiclePartId");

            migrationBuilder.RenameColumn(
                name: "ConsumableItemId",
                table: "MaintenanceActivityDetails",
                newName: "VehiclePartId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceActivityDetails_ConsumableItemId",
                table: "MaintenanceActivityDetails",
                newName: "IX_MaintenanceActivityDetails_VehiclePartId");

            migrationBuilder.AlterColumn<string>(
                name: "VinNumber",
                table: "UserVehicles",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "UserVehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VehiclePartCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VehiclePartCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_VehicleParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleParts_VehiclePartCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "VehiclePartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Oils",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViscosityGrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApiServiceClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JasoRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BaseOilType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecommendedVolumeLiters = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    VehicleUsage = table.Column<int>(type: "integer", nullable: false),
                    RecommendedIntervalKmScooter = table.Column<int>(type: "integer", nullable: true),
                    RecommendedIntervalKmManual = table.Column<int>(type: "integer", nullable: true),
                    RecommendedIntervalMonths = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oils_VehicleParts_VehiclePartId",
                        column: x => x.VehiclePartId,
                        principalTable: "VehicleParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Oils_VehiclePartId",
                table: "Oils",
                column: "VehiclePartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartCategories_Code_Unique",
                table: "VehiclePartCategories",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartCategories_DisplayOrder_Status",
                table: "VehiclePartCategories",
                columns: new[] { "DisplayOrder", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParts_CategoryId_Status",
                table: "VehicleParts",
                columns: new[] { "CategoryId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParts_Name",
                table: "VehicleParts",
                column: "Name",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParts_Sku",
                table: "VehicleParts",
                column: "Sku",
                filter: "\"DeletedAt\" IS NULL AND \"Sku\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceActivityDetails_VehicleParts_VehiclePartId",
                table: "MaintenanceActivityDetails",
                column: "VehiclePartId",
                principalTable: "VehicleParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardMaintenanceSchedules_VehicleParts_VehiclePartId",
                table: "StandardMaintenanceSchedules",
                column: "VehiclePartId",
                principalTable: "VehicleParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMaintenanceConfigs_VehicleParts_VehiclePartId",
                table: "UserMaintenanceConfigs",
                column: "VehiclePartId",
                principalTable: "VehicleParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceActivityDetails_VehicleParts_VehiclePartId",
                table: "MaintenanceActivityDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardMaintenanceSchedules_VehicleParts_VehiclePartId",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMaintenanceConfigs_VehicleParts_VehiclePartId",
                table: "UserMaintenanceConfigs");

            migrationBuilder.DropTable(
                name: "Oils");

            migrationBuilder.DropTable(
                name: "VehicleParts");

            migrationBuilder.DropTable(
                name: "VehiclePartCategories");

            migrationBuilder.DropColumn(
                name: "EngineNumber",
                table: "UserVehicles");

            migrationBuilder.RenameColumn(
                name: "VehiclePartId",
                table: "UserMaintenanceConfigs",
                newName: "ConsumableItemId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMaintenanceConfigs_VehiclePartId",
                table: "UserMaintenanceConfigs",
                newName: "IX_UserMaintenanceConfigs_ConsumableItemId");

            migrationBuilder.RenameColumn(
                name: "VehiclePartId",
                table: "StandardMaintenanceSchedules",
                newName: "ConsumableItemId");

            migrationBuilder.RenameIndex(
                name: "IX_StandardMaintenanceSchedules_VehiclePartId",
                table: "StandardMaintenanceSchedules",
                newName: "IX_StandardMaintenanceSchedules_ConsumableItemId");

            migrationBuilder.RenameIndex(
                name: "IX_StandardMaintenanceSchedules_VehicleModelId_VehiclePartId",
                table: "StandardMaintenanceSchedules",
                newName: "IX_StandardMaintenanceSchedules_VehicleModelId_ConsumableItemId");

            migrationBuilder.RenameColumn(
                name: "VehiclePartId",
                table: "MaintenanceActivityDetails",
                newName: "ConsumableItemId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceActivityDetails_VehiclePartId",
                table: "MaintenanceActivityDetails",
                newName: "IX_MaintenanceActivityDetails_ConsumableItemId");

            migrationBuilder.AlterColumn<string>(
                name: "VinNumber",
                table: "UserVehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(17)",
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ConsumableItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableItems", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceActivityDetails_ConsumableItems_ConsumableItemId",
                table: "MaintenanceActivityDetails",
                column: "ConsumableItemId",
                principalTable: "ConsumableItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardMaintenanceSchedules_ConsumableItems_ConsumableItem~",
                table: "StandardMaintenanceSchedules",
                column: "ConsumableItemId",
                principalTable: "ConsumableItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMaintenanceConfigs_ConsumableItems_ConsumableItemId",
                table: "UserMaintenanceConfigs",
                column: "ConsumableItemId",
                principalTable: "ConsumableItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
