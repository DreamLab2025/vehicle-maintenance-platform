using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCoordinatesNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "GarageBranches",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "GarageBranches",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            // Nullify legacy sentinel values (0, 0) from before this change
            migrationBuilder.Sql("""
                UPDATE "GarageBranches"
                SET "Latitude" = NULL, "Longitude" = NULL
                WHERE "Latitude" = 0 AND "Longitude" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore 0 for rows that have NULL before reverting to non-nullable
            migrationBuilder.Sql("""
                UPDATE "GarageBranches"
                SET "Latitude" = 0, "Longitude" = 0
                WHERE "Latitude" IS NULL OR "Longitude" IS NULL;
                """);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "GarageBranches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldNullable: true,
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "GarageBranches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldNullable: true,
                oldType: "double precision");
        }
    }
}
