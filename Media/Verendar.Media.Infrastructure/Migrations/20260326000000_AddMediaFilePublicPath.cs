using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFilePublicPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicPath",
                table: "Files",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicPath",
                table: "Files");
        }
    }
}
