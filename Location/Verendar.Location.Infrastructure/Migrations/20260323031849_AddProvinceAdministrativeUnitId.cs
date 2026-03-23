using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Location.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceAdministrativeUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdministrativeUnitId",
                table: "Provinces",
                type: "integer",
                nullable: true);

            // Matches seed_data.sql administrative_unit_id (1 = central city, 2 = province).
            migrationBuilder.Sql(
                """
                UPDATE "Provinces" SET "AdministrativeUnitId" = CASE
                    WHEN "Code" IN ('01','31','46','48','79','92') THEN 1
                    ELSE 2
                END
                WHERE "AdministrativeUnitId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "AdministrativeUnitId",
                table: "Provinces",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_AdministrativeUnitId",
                table: "Provinces",
                column: "AdministrativeUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Provinces_AdministrativeUnits_AdministrativeUnitId",
                table: "Provinces",
                column: "AdministrativeUnitId",
                principalTable: "AdministrativeUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Provinces_AdministrativeUnits_AdministrativeUnitId",
                table: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Provinces_AdministrativeUnitId",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "AdministrativeUnitId",
                table: "Provinces");
        }
    }
}
