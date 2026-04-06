using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToGarageAndBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Slug to Garages — fill existing rows with a placeholder derived from BusinessName
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Garages",
                type: "character varying(110)",
                maxLength: 110,
                nullable: false,
                defaultValue: "");

            // Back-fill existing rows so the NOT NULL + unique constraint can be applied
            migrationBuilder.Sql("""
                UPDATE "Garages"
                SET "Slug" = CONCAT('garage-', REPLACE(CAST("Id" AS text), '-', ''))
                WHERE "Slug" = ''
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Garages_Slug",
                table: "Garages",
                column: "Slug",
                unique: true);

            // Drop the temporary default so future inserts must supply Slug explicitly
            migrationBuilder.Sql("""ALTER TABLE "Garages" ALTER COLUMN "Slug" DROP DEFAULT""");

            // Add Slug to GarageBranches
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "GarageBranches",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "GarageBranches"
                SET "Slug" = CONCAT('branch-', REPLACE(CAST("Id" AS text), '-', ''))
                WHERE "Slug" = ''
                """);

            migrationBuilder.CreateIndex(
                name: "IX_GarageBranches_Slug",
                table: "GarageBranches",
                column: "Slug",
                unique: true);

            migrationBuilder.Sql("""ALTER TABLE "GarageBranches" ALTER COLUMN "Slug" DROP DEFAULT""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GarageBranches_Slug",
                table: "GarageBranches");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "GarageBranches");

            migrationBuilder.DropIndex(
                name: "IX_Garages_Slug",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Garages");
        }
    }
}
