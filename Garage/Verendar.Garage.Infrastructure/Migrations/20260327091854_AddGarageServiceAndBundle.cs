using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageServiceAndBundle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GarageProducts_GarageProductId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_GarageProducts_ServiceCategories_ServiceCategoryId",
                table: "GarageProducts");

            migrationBuilder.DropTable(
                name: "GarageProductItems");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_GarageProductId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "GarageProducts");

            migrationBuilder.DropColumn(
                name: "GarageProductId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "Price_Currency",
                table: "GarageProducts",
                newName: "MaterialPrice_Currency");

            migrationBuilder.RenameColumn(
                name: "Price_Amount",
                table: "GarageProducts",
                newName: "MaterialPrice_Amount");

            migrationBuilder.RenameColumn(
                name: "ServiceCategoryId",
                table: "GarageProducts",
                newName: "InstallationServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_GarageProducts_ServiceCategoryId",
                table: "GarageProducts",
                newName: "IX_GarageProducts_InstallationServiceId");

            migrationBuilder.RenameColumn(
                name: "BookedPrice_Currency",
                table: "Bookings",
                newName: "BookedTotalPrice_Currency");

            migrationBuilder.RenameColumn(
                name: "BookedPrice_Amount",
                table: "Bookings",
                newName: "BookedTotalPrice_Amount");

            migrationBuilder.CreateTable(
                name: "GarageBundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GarageBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DiscountAmount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountAmount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_GarageBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageBundles_GarageBranches_GarageBranchId",
                        column: x => x.GarageBranchId,
                        principalTable: "GarageBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GarageServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GarageBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LaborPrice_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LaborPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_GarageServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageServices_GarageBranches_GarageBranchId",
                        column: x => x.GarageBranchId,
                        principalTable: "GarageBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageServices_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookingLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BundleId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncludeInstallation = table.Column<bool>(type: "boolean", nullable: false),
                    BookedItemPrice_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BookedItemPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingLineItems_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingLineItems_GarageBundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "GarageBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingLineItems_GarageProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "GarageProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingLineItems_GarageServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "GarageServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GarageBundleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GarageBundleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncludeInstallation = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageBundleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageBundleItems_GarageBundles_GarageBundleId",
                        column: x => x.GarageBundleId,
                        principalTable: "GarageBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageBundleItems_GarageProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "GarageProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarageBundleItems_GarageServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "GarageServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingLineItems_BookingId",
                table: "BookingLineItems",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLineItems_BundleId",
                table: "BookingLineItems",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLineItems_ProductId",
                table: "BookingLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLineItems_ServiceId",
                table: "BookingLineItems",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageBundleItems_GarageBundleId",
                table: "GarageBundleItems",
                column: "GarageBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageBundleItems_ProductId",
                table: "GarageBundleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageBundleItems_ServiceId",
                table: "GarageBundleItems",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageBundles_GarageBranchId",
                table: "GarageBundles",
                column: "GarageBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageServices_GarageBranchId",
                table: "GarageServices",
                column: "GarageBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageServices_ServiceCategoryId",
                table: "GarageServices",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarageProducts_GarageServices_InstallationServiceId",
                table: "GarageProducts",
                column: "InstallationServiceId",
                principalTable: "GarageServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageProducts_GarageServices_InstallationServiceId",
                table: "GarageProducts");

            migrationBuilder.DropTable(
                name: "BookingLineItems");

            migrationBuilder.DropTable(
                name: "GarageBundleItems");

            migrationBuilder.DropTable(
                name: "GarageBundles");

            migrationBuilder.DropTable(
                name: "GarageServices");

            migrationBuilder.RenameColumn(
                name: "MaterialPrice_Currency",
                table: "GarageProducts",
                newName: "Price_Currency");

            migrationBuilder.RenameColumn(
                name: "MaterialPrice_Amount",
                table: "GarageProducts",
                newName: "Price_Amount");

            migrationBuilder.RenameColumn(
                name: "InstallationServiceId",
                table: "GarageProducts",
                newName: "ServiceCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_GarageProducts_InstallationServiceId",
                table: "GarageProducts",
                newName: "IX_GarageProducts_ServiceCategoryId");

            migrationBuilder.RenameColumn(
                name: "BookedTotalPrice_Currency",
                table: "Bookings",
                newName: "BookedPrice_Currency");

            migrationBuilder.RenameColumn(
                name: "BookedTotalPrice_Amount",
                table: "Bookings",
                newName: "BookedPrice_Amount");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "GarageProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "GarageProductId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "GarageProductItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GarageProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatesTracking = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageProductItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageProductItems_GarageProducts_GarageProductId",
                        column: x => x.GarageProductId,
                        principalTable: "GarageProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GarageProductId",
                table: "Bookings",
                column: "GarageProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageProductItems_GarageProductId",
                table: "GarageProductItems",
                column: "GarageProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GarageProducts_GarageProductId",
                table: "Bookings",
                column: "GarageProductId",
                principalTable: "GarageProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GarageProducts_ServiceCategories_ServiceCategoryId",
                table: "GarageProducts",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
