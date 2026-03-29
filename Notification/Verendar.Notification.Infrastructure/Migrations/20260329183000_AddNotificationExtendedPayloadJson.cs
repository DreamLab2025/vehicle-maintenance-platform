using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationExtendedPayloadJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtendedPayloadJson",
                table: "Notifications",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedPayloadJson",
                table: "Notifications");
        }
    }
}
