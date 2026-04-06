using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVehiclePartProductCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecordItems_PartProducts_PartProductId",
                table: "MaintenanceRecordItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehiclePartTrackings_PartProducts_CurrentPartProductId",
                table: "VehiclePartTrackings");

            migrationBuilder.DropTable(
                name: "PartProducts");

            migrationBuilder.DropIndex(
                name: "IX_VehiclePartTrackings_CurrentPartProductId",
                table: "VehiclePartTrackings");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecordItems_PartProductId",
                table: "MaintenanceRecordItems");

            migrationBuilder.DropColumn(
                name: "PartProductId",
                table: "MaintenanceRecordItems");

            migrationBuilder.AddColumn<Guid>(
                name: "GarageProductId",
                table: "MaintenanceRecordItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CurrentPartProductId",
                table: "VehiclePartTrackings");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentGarageProductId",
                table: "VehiclePartTrackings",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GarageProductId",
                table: "MaintenanceRecordItems");

            migrationBuilder.AddColumn<Guid>(
                name: "PartProductId",
                table: "MaintenanceRecordItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CurrentGarageProductId",
                table: "VehiclePartTrackings");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentPartProductId",
                table: "VehiclePartTrackings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecommendedKmInterval = table.Column<int>(type: "integer", nullable: true),
                    RecommendedMonthsInterval = table.Column<int>(type: "integer", nullable: true),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePartTrackings_CurrentPartProductId",
                table: "VehiclePartTrackings",
                column: "CurrentPartProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecordItems_PartProductId",
                table: "MaintenanceRecordItems",
                column: "PartProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PartProducts_PartCategoryId",
                table: "PartProducts",
                column: "PartCategoryId",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecordItems_PartProducts_PartProductId",
                table: "MaintenanceRecordItems",
                column: "PartProductId",
                principalTable: "PartProducts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehiclePartTrackings_PartProducts_CurrentPartProductId",
                table: "VehiclePartTrackings",
                column: "CurrentPartProductId",
                principalTable: "PartProducts",
                principalColumn: "Id");
        }
    }
}
