using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCurrentToMaintenanceReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "MaintenanceReminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "MaintenanceReminders");
        }
    }
}
