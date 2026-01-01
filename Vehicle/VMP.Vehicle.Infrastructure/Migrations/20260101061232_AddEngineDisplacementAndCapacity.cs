using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VMP.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineDisplacementAndCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EngineCapacity",
                table: "VehicleModels",
                type: "numeric(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EngineDisplacement",
                table: "VehicleModels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineCapacity",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "EngineDisplacement",
                table: "VehicleModels");
        }
    }
}
