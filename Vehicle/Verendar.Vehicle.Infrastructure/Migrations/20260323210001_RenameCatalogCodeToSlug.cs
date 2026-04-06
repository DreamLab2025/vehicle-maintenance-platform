using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    [DbContext(typeof(VehicleDbContext))]
    [Migration("20260323210001_RenameCatalogCodeToSlug")]
    public partial class RenameCatalogCodeToSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "VehicleTypes",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "VehicleBrands",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "VehicleModels",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "PartCategories",
                newName: "Slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "VehicleTypes",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "VehicleBrands",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "VehicleModels",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "PartCategories",
                newName: "Code");
        }
    }
}
