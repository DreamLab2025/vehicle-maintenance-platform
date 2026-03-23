using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGarageAccountToGarage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageBranches_GarageAccounts_GarageAccountId",
                table: "GarageBranches");

            migrationBuilder.RenameTable(
                name: "GarageAccounts",
                newName: "Garages");

            migrationBuilder.RenameIndex(
                name: "IX_GarageAccounts_OwnerId",
                table: "Garages",
                newName: "IX_Garages_OwnerId");

            migrationBuilder.RenameColumn(
                name: "GarageAccountId",
                table: "GarageBranches",
                newName: "GarageId");

            migrationBuilder.RenameIndex(
                name: "IX_GarageBranches_GarageAccountId",
                table: "GarageBranches",
                newName: "IX_GarageBranches_GarageId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarageBranches_Garages_GarageId",
                table: "GarageBranches",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageBranches_Garages_GarageId",
                table: "GarageBranches");

            migrationBuilder.RenameTable(
                name: "Garages",
                newName: "GarageAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_Garages_OwnerId",
                table: "GarageAccounts",
                newName: "IX_GarageAccounts_OwnerId");

            migrationBuilder.RenameColumn(
                name: "GarageId",
                table: "GarageBranches",
                newName: "GarageAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_GarageBranches_GarageId",
                table: "GarageBranches",
                newName: "IX_GarageBranches_GarageAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarageBranches_GarageAccounts_GarageAccountId",
                table: "GarageBranches",
                column: "GarageAccountId",
                principalTable: "GarageAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
