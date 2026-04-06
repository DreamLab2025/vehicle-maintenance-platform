using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemapNotificationPriorityCriticalToHigh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """UPDATE "Notifications" SET "Priority" = 3 WHERE "Priority" = 4;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible: cannot know which High (3) rows were remapped from former Critical (4).
        }
    }
}
