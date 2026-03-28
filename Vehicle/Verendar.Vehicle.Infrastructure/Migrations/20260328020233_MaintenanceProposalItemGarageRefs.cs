using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceProposalItemGarageRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GarageProductId",
                table: "MaintenanceProposalItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GarageServiceId",
                table: "MaintenanceProposalItems",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GarageProductId",
                table: "MaintenanceProposalItems");

            migrationBuilder.DropColumn(
                name: "GarageServiceId",
                table: "MaintenanceProposalItems");
        }
    }
}
