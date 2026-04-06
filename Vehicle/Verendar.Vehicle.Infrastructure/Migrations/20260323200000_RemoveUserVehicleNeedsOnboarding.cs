using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    [DbContext(typeof(VehicleDbContext))]
    [Migration("20260323200000_RemoveUserVehicleNeedsOnboarding")]
    public partial class RemoveUserVehicleNeedsOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsOnboarding",
                table: "UserVehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsOnboarding",
                table: "UserVehicles",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
