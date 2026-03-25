using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseNumberToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_HouseNumber",
                table: "GarageBranches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_HouseNumber",
                table: "GarageBranches");
        }
    }
}
