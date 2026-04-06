using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "ImageUrls",
                table: "Feedbacks",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Feedbacks");
        }
    }
}
