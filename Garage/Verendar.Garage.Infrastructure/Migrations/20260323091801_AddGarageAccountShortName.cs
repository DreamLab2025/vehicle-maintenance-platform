using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageAccountShortName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "GarageAccounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "GarageAccounts");
        }
    }
}
