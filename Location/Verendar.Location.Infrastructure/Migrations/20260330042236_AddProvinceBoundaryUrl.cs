using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Location.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceBoundaryUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoundaryUrl",
                table: "Provinces",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoundaryUrl",
                table: "Provinces");
        }
    }
}
