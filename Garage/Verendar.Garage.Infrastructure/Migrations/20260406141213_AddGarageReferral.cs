using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Garage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Garages",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReferralCodeGeneratedAt",
                table: "Garages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GarageReferrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GarageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferredUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageReferrals_Garages_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Garages_ReferralCode",
                table: "Garages",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarageReferrals_GarageId",
                table: "GarageReferrals",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageReferrals_GarageId_ReferredUserId",
                table: "GarageReferrals",
                columns: new[] { "GarageId", "ReferredUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarageReferrals_ReferredUserId",
                table: "GarageReferrals",
                column: "ReferredUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarageReferrals");

            migrationBuilder.DropIndex(
                name: "IX_Garages_ReferralCode",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "ReferralCodeGeneratedAt",
                table: "Garages");
        }
    }
}
