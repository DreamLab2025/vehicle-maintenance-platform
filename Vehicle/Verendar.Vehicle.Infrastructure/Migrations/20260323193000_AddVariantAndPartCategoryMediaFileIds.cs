using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Verendar.Vehicle.Infrastructure.Data;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    [DbContext(typeof(VehicleDbContext))]
    [Migration("20260323193000_AddVariantAndPartCategoryMediaFileIds")]
    public partial class AddVariantAndPartCategoryMediaFileIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageMediaFileId",
                table: "VehicleVariants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IconMediaFileId",
                table: "PartCategories",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageMediaFileId",
                table: "VehicleVariants");

            migrationBuilder.DropColumn(
                name: "IconMediaFileId",
                table: "PartCategories");
        }
    }
}
