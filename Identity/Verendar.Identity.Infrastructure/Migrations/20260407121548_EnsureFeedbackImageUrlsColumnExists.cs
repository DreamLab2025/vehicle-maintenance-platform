using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureFeedbackImageUrlsColumnExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Feedbacks"
                ADD COLUMN IF NOT EXISTS "ImageUrls" text[] NOT NULL DEFAULT ARRAY[]::text[];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Feedbacks"
                DROP COLUMN IF EXISTS "ImageUrls";
                """);
        }
    }
}
