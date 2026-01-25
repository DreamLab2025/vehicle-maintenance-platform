using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixToSupportFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysisResult",
                table: "VehiclePartTrackings");

            migrationBuilder.DropColumn(
                name: "UserConditionDescription",
                table: "VehiclePartTrackings");

            migrationBuilder.RenameColumn(
                name: "IsIgnored",
                table: "VehiclePartTrackings",
                newName: "IsDeclared");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeclared",
                table: "VehiclePartTrackings",
                newName: "IsIgnored");

            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisResult",
                table: "VehiclePartTrackings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserConditionDescription",
                table: "VehiclePartTrackings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
