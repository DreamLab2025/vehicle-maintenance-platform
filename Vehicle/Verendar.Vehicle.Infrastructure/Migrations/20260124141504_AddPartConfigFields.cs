using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisResult",
                table: "VehiclePartTrackings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomConfigured",
                table: "VehiclePartTrackings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserConditionDescription",
                table: "VehiclePartTrackings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysisResult",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "IsCustomConfigured",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "UserConditionDescription",
                table: "VehiclePartTrackings");
        }
    }
}
