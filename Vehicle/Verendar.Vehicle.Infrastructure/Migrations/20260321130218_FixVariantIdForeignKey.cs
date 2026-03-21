using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixVariantIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF EXISTS guards: depending on how the database was originally created,
            // VariantId (the stale shadow FK column) may or may not already exist.
            migrationBuilder.Sql("""
                ALTER TABLE "UserVehicles" DROP CONSTRAINT IF EXISTS "FK_UserVehicles_VehicleVariants_VariantId";
                DROP INDEX IF EXISTS "IX_UserVehicles_VariantId";
                ALTER TABLE "UserVehicles" DROP COLUMN IF EXISTS "VariantId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VehicleVariantId",
                table: "UserVehicles",
                column: "VehicleVariantId");

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
                name: "FK_UserVehicles_VehicleVariants_VehicleVariantId",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_VehicleVariantId",
                table: "UserVehicles");

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "UserVehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_VariantId",
                table: "UserVehicles",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVehicles_VehicleVariants_VariantId",
                table: "UserVehicles",
                column: "VariantId",
                principalTable: "VehicleVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
