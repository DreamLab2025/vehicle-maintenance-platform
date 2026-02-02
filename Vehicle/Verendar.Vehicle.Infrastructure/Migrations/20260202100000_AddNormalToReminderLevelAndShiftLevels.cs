using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Enum ReminderLevel: thêm Normal=0, shift Low->1, Medium->2, High->3, Urgent->4.
    /// Dữ liệu cũ: 0,1,2,3 → 1,2,3,4.
    /// </summary>
    public partial class AddNormalToReminderLevelAndShiftLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Shift existing levels: 0->1, 1->2, 2->3, 3->4 (old Low,Medium,High,Urgent → new Low,Medium,High,Urgent)
            migrationBuilder.Sql(@"
                UPDATE ""MaintenanceReminders""
                SET ""Level"" = ""Level"" + 1
                WHERE ""DeletedAt"" IS NULL;
            ");

            // Delete duplicate reminders per VehiclePartTrackingId, keep the one with highest Level
            migrationBuilder.Sql(@"
                DELETE FROM ""MaintenanceReminders"" r1
                USING ""MaintenanceReminders"" r2
                WHERE r1.""VehiclePartTrackingId"" = r2.""VehiclePartTrackingId""
                  AND r1.""Id"" != r2.""Id""
                  AND r1.""Level"" < r2.""Level""
                  AND r1.""DeletedAt"" IS NULL
                  AND r2.""DeletedAt"" IS NULL;
            ");

            // If same Level, keep one (lowest Id) - handle remaining duplicates
            migrationBuilder.Sql(@"
                DELETE FROM ""MaintenanceReminders"" r1
                USING ""MaintenanceReminders"" r2
                WHERE r1.""VehiclePartTrackingId"" = r2.""VehiclePartTrackingId""
                  AND r1.""Id"" > r2.""Id""
                  AND r1.""Level"" = r2.""Level""
                  AND r1.""DeletedAt"" IS NULL
                  AND r2.""DeletedAt"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Shift back: 1->0, 2->1, 3->2, 4->3 (cannot restore deleted duplicates)
            migrationBuilder.Sql(@"
                UPDATE ""MaintenanceReminders""
                SET ""Level"" = ""Level"" - 1
                WHERE ""DeletedAt"" IS NULL AND ""Level"" > 0;
            ");
        }
    }
}
