using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verender.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransmissionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransmissionType",
                table: "VehicleModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransmissionType",
                table: "VehicleModels");
        }
    }
}
