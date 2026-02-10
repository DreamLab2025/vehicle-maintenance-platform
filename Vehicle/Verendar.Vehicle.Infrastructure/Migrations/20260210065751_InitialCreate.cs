using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequiresOdometerTracking = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresTimeTracking = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsMultipleInstances = table.Column<bool>(type: "boolean", nullable: false),
                    IdentificationSigns = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConsequencesIfNotHandled = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                name: "PartProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RecommendedKmInterval = table.Column<int>(type: "integer", nullable: true),
                    RecommendedMonthsInterval = table.Column<int>(type: "integer", nullable: true),
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
                name: "VehicleBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.ForeignKey(
                        name: "FK_VehicleBrands_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleBrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: true),
                    FuelType = table.Column<int>(type: "integer", nullable: true),
                    TransmissionType = table.Column<int>(type: "integer", nullable: true),
                    EngineDisplacement = table.Column<int>(type: "integer", nullable: true),
                    EngineCapacity = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                        name: "FK_VehicleModels_VehicleBrands_VehicleBrandId",
                        column: x => x.VehicleBrandId,
                        principalTable: "VehicleBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "VehicleVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HexCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleVariants_VehicleModels_VehicleModelId",
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
                    VehicleVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VIN = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrentOdometer = table.Column<int>(type: "integer", nullable: false),
                    LastOdometerUpdate = table.Column<DateOnly>(type: "date", nullable: true),
                    AverageKmPerDay = table.Column<int>(type: "integer", nullable: true),
                    NeedsOnboarding = table.Column<bool>(type: "boolean", nullable: false),
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
                        name: "FK_UserVehicles_VehicleVariants_VehicleVariantId",
                        column: x => x.VehicleVariantId,
                        principalTable: "VehicleVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OdometerAtService = table.Column<int>(type: "integer", nullable: false),
                    GarageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                name: "OdometerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OdometerValue = table.Column<int>(type: "integer", nullable: false),
                    RecordedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    KmOnRecordedDate = table.Column<int>(type: "integer", nullable: true),
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
                    IsDeclared = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "MaintenanceRecordItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstanceIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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

            migrationBuilder.InsertData(
                table: "PartCategories",
                columns: new[] { "Id", "AllowsMultipleInstances", "Code", "ConsequencesIfNotHandled", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconUrl", "IdentificationSigns", "Name", "RequiresOdometerTracking", "RequiresTimeTracking", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("c0000001-0000-0000-0000-000000000001"), false, "ENGINE-OIL", "Mài mòn nhanh, kẹt piston; hỏng động cơ; chi phí sửa chữa rất cao hoặc phải thay máy.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Dầu bôi trơn động cơ, cần thay định kỳ theo km hoặc thời gian", 1, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Dầu đen, nhớt nhớt; động cơ nóng bất thường; đèn báo dầu sáng; tiếng kêu kim loại từ động cơ.", "Dầu nhớt động cơ", true, true, 1, null, null },
                    { new Guid("c0000002-0000-0000-0000-000000000002"), true, "TIRE", "Trượt, mất lái khi phanh/trời mưa; nổ lốp khi chạy tốc độ cao; tai nạn nghiêm trọng.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Lốp trước và lốp sau, thay khi mòn hoặc đạt tuổi thọ", 2, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Gai lốp mòn dưới 1.6mm; nứt rạn; phồng lốp; lốp non hơi thường xuyên; rung lái khi chạy.", "Lốp xe", true, true, 1, null, null },
                    { new Guid("c0000003-0000-0000-0000-000000000003"), false, "BATTERY", "Chết máy giữa đường; hỏng bình; ảnh hưởng bộ sạc và thiết bị điện; không khởi động được.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bình điện ắc quy, thay khi hết tuổi thọ hoặc không giữ điện", 3, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Khởi động yếu hoặc không nổ; đèn pha mờ; ắc quy phồng, rỉ nước; xe để vài ngày là hết điện.", "Ắc quy", false, true, 1, null, null },
                    { new Guid("c0000004-0000-0000-0000-000000000004"), true, "BRAKE-PAD", "Mất phanh; đĩa phanh hỏng theo; va chạm, tai nạn do không dừng kịp.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Má phanh trước và sau, thay khi mòn", 4, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Tiếng kêu ken két khi phanh; phanh không ăn; tay phanh/ pedal bị trễ; đĩa phanh xước, rỗ.", "Má phanh", true, true, 1, null, null },
                    { new Guid("c0000005-0000-0000-0000-000000000005"), false, "SPARK-PLUG", "Chết máy; đánh lửa kém gây cháy không hết nhiên liệu, hỏng cat; hao xăng, giảm công suất.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bugi đánh lửa động cơ, thay định kỳ", 5, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Khó nổ, giật khi tăng ga; tốn xăng; động cơ rung; bugi đen, dính dầu hoặc cháy trắng đầu điện cực.", "Bugi", true, true, 1, null, null },
                    { new Guid("c0000006-0000-0000-0000-000000000006"), false, "AIR-FILTER", "Bụi cát vào buồng đốt, mài mòn xy-lanh; giảm công suất; hỏng bugi, hao xăng lâu dài.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Lọc không khí động cơ, cần vệ sinh và thay định kỳ", 6, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Xe yếu, không bốc; tốn xăng; lọc gió bẩn, rách hoặc ẩm mốc; động cơ nổ không đều.", "Lọc gió", true, true, 1, null, null },
                    { new Guid("c0000007-0000-0000-0000-000000000007"), false, "CHAIN-SPROCKET", "Đứt xích khi chạy, bó bánh; hỏng nhông, moay-ơ; nguy cơ té xe, hỏng hộp số.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Bộ truyền động xích và nhông, thay khi mòn", 7, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Xích kêu lạch cạch; xích trùng dù đã chỉnh; răng nhông mòn vẹt; xích rỉ, khô dầu.", "Nhông sên dĩa", true, false, 1, null, null },
                    { new Guid("c0000008-0000-0000-0000-000000000008"), false, "BRAKE-FLUID", "Sôi dầu khi phanh gấp; mất phanh; hỏng xy-lanh phanh; tai nạn do không dừng kịp.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Dầu phanh thủy lực, thay định kỳ theo thời gian hoặc km", 8, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Dầu đổi màu đen hoặc vẩn đục; mức dầu thấp; phanh mềm hoặc trễ; có nước lẫn vào.", "Dầu phanh", true, true, 1, null, null },
                    { new Guid("c0000009-0000-0000-0000-000000000009"), false, "OIL-FILTER", "Dầu không lọc sạch, mài mòn động cơ; tắc lọc gây thiếu dầu; hỏng máy.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Lọc dầu động cơ, thay kèm mỗi lần thay dầu nhớt", 9, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Thay cùng chu kỳ dầu nhớt; lọc tắc khi đèn dầu sáng thường xuyên; dầu đen nhanh bất thường.", "Lọc dầu", true, true, 1, null, null },
                    { new Guid("c0000010-0000-0000-0000-000000000010"), false, "COOLANT", "Động cơ quá nhiệt; bó máy; hỏng gioăng, xy-lanh; chi phí sửa rất cao.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Dung dịch làm mát động cơ, kiểm tra và thay định kỳ", 10, "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp", "Mức nước trong bình thấp; nước đổi màu rỉ; động cơ nóng quá; rò rỉ dưới gầm.", "Nước làm mát", true, true, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ImageUrl", "Name", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "MOTORCYCLE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Xe máy hai bánh, bao gồm xe số, xe tay ga, xe côn tay", "https://d3iova6424vljy.cloudfront.net/assets/c51a1e61-9aac-4d59-a01c-5b615be9e794.jpg", "Xe máy", 1, null, null });

            migrationBuilder.InsertData(
                table: "VehicleBrands",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "LogoUrl", "Name", "Status", "SupportPhone", "UpdatedAt", "UpdatedBy", "VehicleTypeId", "Website" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), "HONDA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png", "Honda", 1, "18001076", null, null, new Guid("11111111-1111-1111-1111-111111111111"), "https://www.honda.com.vn" },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), "YAMAHA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png", "Yamaha", 1, "18006019", null, null, new Guid("11111111-1111-1111-1111-111111111111"), "https://www.yamaha-motor.com.vn" }
                });

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
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "InitialKm", "KmInterval", "MonthsInterval", "PartCategoryId", "Status", "UpdatedAt", "UpdatedBy", "VehicleModelId" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 1000, 3000, 4, new Guid("c0000001-0000-0000-0000-000000000001"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 20000, 36, new Guid("c0000002-0000-0000-0000-000000000002"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 0, 24, new Guid("c0000003-0000-0000-0000-000000000003"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 10000, 12, new Guid("c0000004-0000-0000-0000-000000000004"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 8000, 12, new Guid("c0000005-0000-0000-0000-000000000005"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000006-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 6000, 6, new Guid("c0000006-0000-0000-0000-000000000006"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000007-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 15000, 0, new Guid("c0000007-0000-0000-0000-000000000007"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000008-0000-0000-0000-000000000008"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 20000, 24, new Guid("c0000008-0000-0000-0000-000000000008"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000009-0000-0000-0000-000000000009"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 1000, 3000, 4, new Guid("c0000009-0000-0000-0000-000000000009"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("d0000010-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, 20000, 24, new Guid("c0000010-0000-0000-0000-000000000010"), 1, null, null, new Guid("a0000001-0000-0000-0000-000000000001") }
                });

            migrationBuilder.InsertData(
                table: "VehicleVariants",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "HexCode", "ImageUrl", "UpdatedAt", "UpdatedBy", "VehicleModelId" },
                values: new object[,]
                {
                    { new Guid("e0000001-0000-0000-0000-000000000001"), "Đỏ đen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#DC143C", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("e0000002-0000-0000-0000-000000000002"), "Đen bạc", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000000", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000001-0000-0000-0000-000000000001") },
                    { new Guid("e0000003-0000-0000-0000-000000000003"), "Trắng ngọc trai", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#FFFFFF", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000002-0000-0000-0000-000000000002") },
                    { new Guid("e0000004-0000-0000-0000-000000000004"), "Xanh dương", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#4169E1", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000002-0000-0000-0000-000000000002") },
                    { new Guid("e0000005-0000-0000-0000-000000000005"), "Đen nhám", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#1C1C1C", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000003-0000-0000-0000-000000000003") },
                    { new Guid("e0000006-0000-0000-0000-000000000006"), "Đỏ GP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#FF0000", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000003-0000-0000-0000-000000000003") },
                    { new Guid("e0000007-0000-0000-0000-000000000007"), "Xanh đen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000080", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000004-0000-0000-0000-000000000004") },
                    { new Guid("e0000008-0000-0000-0000-000000000008"), "Đen nhám", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "#000000", "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png", null, null, new Guid("a0000004-0000-0000-0000-000000000004") }
                });

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
                name: "IX_OdometerHistories_UserVehicleId_RecordedDate",
                table: "OdometerHistories",
                columns: new[] { "UserVehicleId", "RecordedDate" },
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
                name: "IX_UserVehicles_UserId_Status",
                table: "UserVehicles",
                columns: new[] { "UserId", "Status" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VehicleVariantId",
                table: "UserVehicles",
                column: "VehicleVariantId");

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

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Code",
                table: "VehicleTypes",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_VehicleModelId",
                table: "VehicleVariants",
                column: "VehicleModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultMaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "MaintenanceRecordItems");

            migrationBuilder.DropTable(
                name: "MaintenanceReminders");

            migrationBuilder.DropTable(
                name: "OdometerHistories");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "VehiclePartTrackings");

            migrationBuilder.DropTable(
                name: "PartProducts");

            migrationBuilder.DropTable(
                name: "UserVehicles");

            migrationBuilder.DropTable(
                name: "PartCategories");

            migrationBuilder.DropTable(
                name: "VehicleVariants");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropTable(
                name: "VehicleBrands");

            migrationBuilder.DropTable(
                name: "VehicleTypes");
        }
    }
}
