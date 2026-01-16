using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verender.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModelImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "VehicleModels");

            migrationBuilder.CreateTable(
                name: "ModelImages",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "VehicleModels",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
