using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verender.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameToVehicleVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles");

            migrationBuilder.DropTable(
                name: "ModelImages");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleModelId",
                table: "UserVehicles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleVariantId",
                table: "UserVehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VehicleVariantId",
                table: "UserVehicles",
                column: "VehicleVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_VehicleModelId_Color",
                table: "VehicleVariants",
                columns: new[] { "VehicleModelId", "Color" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVehicles_VehicleVariants_VehicleVariantId",
                table: "UserVehicles",
                column: "VehicleVariantId",
                principalTable: "VehicleVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVehicles_VehicleVariants_VehicleVariantId",
                table: "UserVehicles");

            migrationBuilder.DropTable(
                name: "VehicleVariants");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_VehicleVariantId",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "VehicleVariantId",
                table: "UserVehicles");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleModelId",
                table: "UserVehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ModelImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    HexCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelImages_VehicleModels_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelImages_VehicleModelId_Color",
                table: "ModelImages",
                columns: new[] { "VehicleModelId", "Color" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserVehicles_VehicleModels_VehicleModelId",
                table: "UserVehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
