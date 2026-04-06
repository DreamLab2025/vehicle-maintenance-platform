using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Ai.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantInitialAiPromptHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "AiPromptHistories" AS h
                USING "AiPrompts" AS p
                WHERE h."AiPromptId" = p."Id"
                  AND h."VersionNumber" = 1
                  AND p."VersionNumber" = 1
                  AND h."Note" = 'Initial seed';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only migration; cannot restore deleted history rows.
        }
    }
}
