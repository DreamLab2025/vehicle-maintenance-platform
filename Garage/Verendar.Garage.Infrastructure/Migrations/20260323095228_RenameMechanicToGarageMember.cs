using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMechanicToGarageMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK referencing old table name before rename
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Mechanics_MechanicId",
                table: "Bookings");

            // Rename table (preserves all data)
            migrationBuilder.RenameTable(
                name: "Mechanics",
                newName: "GarageMembers");

            // Rename PK and FK constraints
            migrationBuilder.Sql("ALTER TABLE \"GarageMembers\" RENAME CONSTRAINT \"PK_Mechanics\" TO \"PK_GarageMembers\"");
            migrationBuilder.Sql("ALTER TABLE \"GarageMembers\" RENAME CONSTRAINT \"FK_Mechanics_GarageBranches_GarageBranchId\" TO \"FK_GarageMembers_GarageBranches_GarageBranchId\"");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_Mechanics_GarageBranchId",
                newName: "IX_GarageMembers_GarageBranchId",
                table: "GarageMembers");

            migrationBuilder.RenameIndex(
                name: "IX_Mechanics_UserId",
                newName: "IX_GarageMembers_UserId",
                table: "GarageMembers");

            // Add Role column — default 1 (Mechanic) fills existing rows
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "GarageMembers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Remove the column default so future inserts must supply Role explicitly
            migrationBuilder.Sql("ALTER TABLE \"GarageMembers\" ALTER COLUMN \"Role\" DROP DEFAULT");

            // Add unique active-membership index (from GarageMemberConfiguration)
            migrationBuilder.CreateIndex(
                name: "IX_GarageMembers_GarageBranchId_UserId",
                table: "GarageMembers",
                columns: new[] { "GarageBranchId", "UserId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            // Re-add FK from Bookings pointing to renamed table
            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GarageMembers_MechanicId",
                table: "Bookings",
                column: "MechanicId",
                principalTable: "GarageMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GarageMembers_MechanicId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_GarageMembers_GarageBranchId_UserId",
                table: "GarageMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "GarageMembers");

            migrationBuilder.RenameTable(
                name: "GarageMembers",
                newName: "Mechanics");

            migrationBuilder.Sql("ALTER TABLE \"Mechanics\" RENAME CONSTRAINT \"PK_GarageMembers\" TO \"PK_Mechanics\"");
            migrationBuilder.Sql("ALTER TABLE \"Mechanics\" RENAME CONSTRAINT \"FK_GarageMembers_GarageBranches_GarageBranchId\" TO \"FK_Mechanics_GarageBranches_GarageBranchId\"");

            migrationBuilder.RenameIndex(
                name: "IX_GarageMembers_GarageBranchId",
                newName: "IX_Mechanics_GarageBranchId",
                table: "Mechanics");

            migrationBuilder.RenameIndex(
                name: "IX_GarageMembers_UserId",
                newName: "IX_Mechanics_UserId",
                table: "Mechanics");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Mechanics_MechanicId",
                table: "Bookings",
                column: "MechanicId",
                principalTable: "Mechanics",
                principalColumn: "Id");
        }
    }
}
