using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPhoneNumberConfirmed",
                table: "Users",
                newName: "PhoneNumberVerified");

            migrationBuilder.RenameColumn(
                name: "IsEmailConfirmed",
                table: "Users",
                newName: "EmailVerified");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumberVerified",
                table: "Users",
                newName: "IsPhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "EmailVerified",
                table: "Users",
                newName: "IsEmailConfirmed");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
