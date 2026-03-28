using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleSnapshotToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VehicleSnapshotJson",
                table: "Bookings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleSnapshotJson",
                table: "Bookings");
        }
    }
}
