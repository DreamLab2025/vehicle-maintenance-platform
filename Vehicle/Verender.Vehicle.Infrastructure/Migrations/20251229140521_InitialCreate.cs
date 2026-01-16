using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verender.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumableItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ConsumableItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SupportPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_VehicleBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                    FuelType = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OilCapacity = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    TireSizeFront = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TireSizeRear = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_VehicleModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleModels_VehicleBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "VehicleBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleModels_VehicleTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StandardMaintenanceSchedules",
                columns: table => new
                {
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumableItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitialDistance = table.Column<int>(type: "integer", nullable: true),
                    DistanceInterval = table.Column<int>(type: "integer", nullable: false),
                    TimeIntervalMonth = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardMaintenanceSchedules", x => new { x.VehicleModelId, x.ConsumableItemId });
                    table.ForeignKey(
                        name: "FK_StandardMaintenanceSchedules_ConsumableItems_ConsumableItem~",
                        column: x => x.ConsumableItemId,
                        principalTable: "ConsumableItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StandardMaintenanceSchedules_VehicleModels_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VinNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentOdometer = table.Column<int>(type: "integer", nullable: false),
                    LastOdometerUpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AverageKmPerDay = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LastCalculatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_UserVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OdometerAtTime = table.Column<int>(type: "integer", nullable: false),
                    GarageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "OdometerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OdometerValue = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdometerHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdometerHistories_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMaintenanceConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumableItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastServiceOdometer = table.Column<int>(type: "integer", nullable: false),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomDistanceInterval = table.Column<int>(type: "integer", nullable: true),
                    CustomTimeIntervalMonth = table.Column<int>(type: "integer", nullable: true),
                    PredictedDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsIgnored = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaintenanceConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceConfigs_ConsumableItems_ConsumableItemId",
                        column: x => x.ConsumableItemId,
                        principalTable: "ConsumableItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceConfigs_UserVehicles_UserVehicleId",
                        column: x => x.UserVehicleId,
                        principalTable: "UserVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceActivityDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumableItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceActivityDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceActivityDetails_ConsumableItems_ConsumableItemId",
                        column: x => x.ConsumableItemId,
                        principalTable: "ConsumableItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceActivityDetails_MaintenanceActivities_Maintenanc~",
                        column: x => x.MaintenanceActivityId,
                        principalTable: "MaintenanceActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivities_UserVehicleId",
                table: "MaintenanceActivities",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivityDetails_ConsumableItemId",
                table: "MaintenanceActivityDetails",
                column: "ConsumableItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActivityDetails_MaintenanceActivityId",
                table: "MaintenanceActivityDetails",
                column: "MaintenanceActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerHistories_UserVehicleId",
                table: "OdometerHistories",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardMaintenanceSchedules_ConsumableItemId",
                table: "StandardMaintenanceSchedules",
                column: "ConsumableItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceConfigs_ConsumableItemId",
                table: "UserMaintenanceConfigs",
                column: "ConsumableItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceConfigs_UserVehicleId",
                table: "UserMaintenanceConfigs",
                column: "UserVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VehicleModelId",
                table: "UserVehicles",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandId",
                table: "VehicleModels",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_TypeId",
                table: "VehicleModels",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceActivityDetails");

            migrationBuilder.DropTable(
                name: "OdometerHistories");

            migrationBuilder.DropTable(
                name: "StandardMaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "UserMaintenanceConfigs");

            migrationBuilder.DropTable(
                name: "MaintenanceActivities");

            migrationBuilder.DropTable(
                name: "ConsumableItems");

            migrationBuilder.DropTable(
                name: "UserVehicles");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropTable(
                name: "VehicleBrands");

            migrationBuilder.DropTable(
                name: "VehicleTypes");
        }
    }
}
