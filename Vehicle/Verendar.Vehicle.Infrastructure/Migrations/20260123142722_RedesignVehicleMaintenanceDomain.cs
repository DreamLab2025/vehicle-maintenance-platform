using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignVehicleMaintenanceDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandId",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleTypes_TypeId",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleTypeBrands_VehicleBrands_VehicleBrandId",
                table: "VehicleTypeBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleTypeBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleTypeBrands");

            migrationBuilder.DropTable(
                name: "MaintenanceActivityDetails");

            migrationBuilder.DropTable(
                name: "Oils");

            migrationBuilder.DropTable(
                name: "StandardMaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "UserMaintenanceConfigs");

            migrationBuilder.DropTable(
                name: "MaintenanceActivities");

            migrationBuilder.DropTable(
                name: "VehicleParts");

            migrationBuilder.DropTable(
                name: "VehiclePartCategories");

            migrationBuilder.DropIndex(
                name: "IX_VehicleVariants_VehicleModelId_Color",
                table: "VehicleVariants");

            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_BrandId",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_TypeId",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_VehicleModelId",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_OdometerHistories_UserVehicleId",
                table: "OdometerHistories");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "OilCapacity",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "TireSizeFront",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "TireSizeRear",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "LastCalculatedDate",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "LastOdometerUpdateAt",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "VehicleModelId",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "RecordedAt",
                table: "OdometerHistories");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "VehicleModels",
                newName: "VehicleBrandId");

            migrationBuilder.RenameColumn(
                name: "VinNumber",
                table: "UserVehicles",
                newName: "VIN");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "VehicleTypes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "TransmissionType",
                table: "VehicleModels",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "FuelType",
                table: "VehicleModels",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "VehicleModels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "VehicleModels",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManufactureYear",
                table: "VehicleModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "VehicleBrands",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleTypeId",
                table: "VehicleBrands",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PurchaseDate",
                table: "UserVehicles",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "UserVehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "EngineNumber",
                table: "UserVehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "AverageKmPerDay",
                table: "UserVehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastOdometerUpdate",
                table: "UserVehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserVehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RecordedDate",
                table: "OdometerHistories",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OdometerAtService = table.Column<int>(type: "integer", nullable: false),
                    GarageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GarageAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TechnicianName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InvoiceImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiresOdometerTracking = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresTimeTracking = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsMultipleInstances = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_PartCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultMaintenanceSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitialKm = table.Column<int>(type: "integer", nullable: false),
                    KmInterval = table.Column<int>(type: "integer", nullable: false),
                    MonthsInterval = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_DefaultMaintenanceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultMaintenanceSchedules_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefaultMaintenanceSchedules_VehicleModels_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SKU = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PriceCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecommendedKmInterval = table.Column<int>(type: "integer", nullable: true),
                    RecommendedMonthsInterval = table.Column<int>(type: "integer", nullable: true),
                    Specifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PartProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartProducts_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecordItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstanceIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatesTracking = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecordItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecordItems_MaintenanceRecords_MaintenanceRecord~",
                        column: x => x.MaintenanceRecordId,
                        principalTable: "MaintenanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecordItems_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecordItems_PartProducts_PartProductId",
                        column: x => x.PartProductId,
                        principalTable: "PartProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehiclePartTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPartProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstanceIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastReplacementOdometer = table.Column<int>(type: "integer", nullable: true),
                    LastReplacementDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CustomKmInterval = table.Column<int>(type: "integer", nullable: true),
                    CustomMonthsInterval = table.Column<int>(type: "integer", nullable: true),
                    PredictedNextOdometer = table.Column<int>(type: "integer", nullable: true),
                    PredictedNextDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsIgnored = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_VehiclePartTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclePartTrackings_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehiclePartTrackings_PartProducts_CurrentPartProductId",
                        column: x => x.CurrentPartProductId,
                        principalTable: "PartProducts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehiclePartTrackings_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartTrackingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentOdometer = table.Column<int>(type: "integer", nullable: false),
                    TargetOdometer = table.Column<int>(type: "integer", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    PercentageRemaining = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsNotified = table.Column<bool>(type: "boolean", nullable: false),
                    NotifiedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    DismissedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceReminders_VehiclePartTrackings_VehiclePartTracki~",
                        column: x => x.VehiclePartTrackingId,
                        principalTable: "VehiclePartTrackings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Code",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Code",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Code",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_VehicleModelId",
                table: "VehicleVariants",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Code",
                table: "VehicleTypes",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_Code",
                table: "VehicleModels",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_VehicleBrandId_Status",
                table: "VehicleModels",
                columns: new[] { "VehicleBrandId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBrands_Code",
                table: "VehicleBrands",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBrands_VehicleTypeId",
                table: "VehicleBrands",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_UserId_Status",
                table: "UserVehicles",
                columns: new[] { "UserId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerHistories_UserVehicleId_RecordedDate",
                table: "OdometerHistories",
                columns: new[] { "UserVehicleId", "RecordedDate" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultMaintenanceSchedules_PartCategoryId",
                table: "DefaultMaintenanceSchedules",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultMaintenanceSchedules_VehicleModelId_PartCategoryId",
                table: "DefaultMaintenanceSchedules",
                columns: new[] { "VehicleModelId", "PartCategoryId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecordItems_MaintenanceRecordId",
                table: "MaintenanceRecordItems",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecordItems_PartCategoryId",
                table: "MaintenanceRecordItems",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecordItems_PartProductId",
                table: "MaintenanceRecordItems",
                column: "PartProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_UserVehicleId_ServiceDate",
                table: "MaintenanceRecords",
                columns: new[] { "UserVehicleId", "ServiceDate" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceReminders_VehiclePartTrackingId_Level",
                table: "MaintenanceReminders",
                columns: new[] { "VehiclePartTrackingId", "Level" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_Code",
                table: "PartCategories",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_DisplayOrder_Status",
                table: "PartCategories",
                columns: new[] { "DisplayOrder", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartProducts_PartCategoryId_Status",
                table: "PartProducts",
                columns: new[] { "PartCategoryId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartTrackings_CurrentPartProductId",
                table: "VehiclePartTrackings",
                column: "CurrentPartProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartTrackings_PartCategoryId",
                table: "VehiclePartTrackings",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartTrackings_UserVehicleId_PartCategoryId_InstanceI~",
                table: "VehiclePartTrackings",
                columns: new[] { "UserVehicleId", "PartCategoryId", "InstanceIdentifier" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleBrands",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehicleBrandId",
                table: "VehicleModels",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleTypeBrands_VehicleBrands_VehicleBrandId",
                table: "VehicleTypeBrands",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleTypeBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleTypeBrands",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehicleBrandId",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleTypeBrands_VehicleBrands_VehicleBrandId",
                table: "VehicleTypeBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleTypeBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleTypeBrands");

            migrationBuilder.DropTable(
                name: "DefaultMaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "MaintenanceRecordItems");

            migrationBuilder.DropTable(
                name: "MaintenanceReminders");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "VehiclePartTrackings");

            migrationBuilder.DropTable(
                name: "PartProducts");

            migrationBuilder.DropTable(
                name: "PartCategories");

            migrationBuilder.DropIndex(
                name: "IX_VehicleVariants_VehicleModelId",
                table: "VehicleVariants");

            migrationBuilder.DropIndex(
                name: "IX_VehicleTypes_Code",
                table: "VehicleTypes");

            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_Code",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_VehicleModels_VehicleBrandId_Status",
                table: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_VehicleBrands_Code",
                table: "VehicleBrands");

            migrationBuilder.DropIndex(
                name: "IX_VehicleBrands_VehicleTypeId",
                table: "VehicleBrands");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_UserId_Status",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_OdometerHistories_UserVehicleId_RecordedDate",
                table: "OdometerHistories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "ManufactureYear",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "VehicleBrands");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "VehicleBrands");

            migrationBuilder.DropColumn(
                name: "LastOdometerUpdate",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "RecordedDate",
                table: "OdometerHistories");

            migrationBuilder.RenameColumn(
                name: "VehicleBrandId",
                table: "VehicleModels",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "VIN",
                table: "UserVehicles",
                newName: "VinNumber");

            migrationBuilder.AlterColumn<int>(
                name: "TransmissionType",
                table: "VehicleModels",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FuelType",
                table: "VehicleModels",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "VehicleModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "OilCapacity",
                table: "VehicleModels",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "VehicleModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TireSizeFront",
                table: "VehicleModels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TireSizeRear",
                table: "VehicleModels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchaseDate",
                table: "UserVehicles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "UserVehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EngineNumber",
                table: "UserVehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageKmPerDay",
                table: "UserVehicles",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCalculatedDate",
                table: "UserVehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOdometerUpdateAt",
                table: "UserVehicles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "UserVehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleModelId",
                table: "UserVehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordedAt",
                table: "OdometerHistories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "MaintenanceActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GarageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OdometerAtTime = table.Column<int>(type: "integer", nullable: false),
                    PerformedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceActivities_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePartCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
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
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "MaintenanceActivityDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceActivityDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceActivityDetails_MaintenanceActivities_Maintenanc~",
                        column: x => x.MaintenanceActivityId,
                        principalTable: "MaintenanceActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceActivityDetails_VehicleParts_VehiclePartId",
                        column: x => x.VehiclePartId,
                        principalTable: "VehicleParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oils",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiServiceClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BaseOilType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    JasoRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecommendedIntervalKmManual = table.Column<int>(type: "integer", nullable: true),
                    RecommendedIntervalKmScooter = table.Column<int>(type: "integer", nullable: true),
                    RecommendedIntervalMonths = table.Column<int>(type: "integer", nullable: true),
                    RecommendedVolumeLiters = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleUsage = table.Column<int>(type: "integer", nullable: false),
                    ViscosityGrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "StandardMaintenanceSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DistanceInterval = table.Column<int>(type: "integer", nullable: false),
                    InitialDistance = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TimeIntervalMonth = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardMaintenanceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardMaintenanceSchedules_VehicleModels_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StandardMaintenanceSchedules_VehicleParts_VehiclePartId",
                        column: x => x.VehiclePartId,
                        principalTable: "VehicleParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMaintenanceConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomDistanceInterval = table.Column<int>(type: "integer", nullable: true),
                    CustomTimeIntervalMonth = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsIgnored = table.Column<bool>(type: "boolean", nullable: false),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastServiceOdometer = table.Column<int>(type: "integer", nullable: false),
                    PredictedDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaintenanceConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceConfigs_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceConfigs_VehicleParts_VehiclePartId",
                        column: x => x.VehiclePartId,
                        principalTable: "VehicleParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_VehicleModelId_Color",
                table: "VehicleVariants",
                columns: new[] { "VehicleModelId", "Color" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandId",
                table: "VehicleModels",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_TypeId",
                table: "VehicleModels",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VehicleModelId",
                table: "UserVehicles",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerHistories_UserVehicleId",
                table: "OdometerHistories",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivities_UserVehicleId",
                table: "MaintenanceActivities",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivityDetails_MaintenanceActivityId",
                table: "MaintenanceActivityDetails",
                column: "MaintenanceActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivityDetails_VehiclePartId",
                table: "MaintenanceActivityDetails",
                column: "VehiclePartId");

            migrationBuilder.CreateIndex(
                name: "IX_Oils_VehiclePartId",
                table: "Oils",
                column: "VehiclePartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardMaintenanceSchedules_VehicleModelId_VehiclePartId",
                table: "StandardMaintenanceSchedules",
                columns: new[] { "VehicleModelId", "VehiclePartId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardMaintenanceSchedules_VehiclePartId",
                table: "StandardMaintenanceSchedules",
                column: "VehiclePartId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceConfigs_UserVehicleId",
                table: "UserMaintenanceConfigs",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceConfigs_VehiclePartId",
                table: "UserMaintenanceConfigs",
                column: "VehiclePartId");

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
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandId",
                table: "VehicleModels",
                column: "BrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleTypes_TypeId",
                table: "VehicleModels",
                column: "TypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleTypeBrands_VehicleBrands_VehicleBrandId",
                table: "VehicleTypeBrands",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleTypeBrands_VehicleTypes_VehicleTypeId",
                table: "VehicleTypeBrands",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
