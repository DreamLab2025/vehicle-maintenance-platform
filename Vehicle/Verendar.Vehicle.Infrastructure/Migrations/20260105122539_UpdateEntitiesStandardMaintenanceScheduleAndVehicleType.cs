using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesStandardMaintenanceScheduleAndVehicleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StandardMaintenanceSchedules",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "VehicleTypeBrands");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VehicleTypes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "StandardMaintenanceSchedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StandardMaintenanceSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StandardMaintenanceSchedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StandardMaintenanceSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "StandardMaintenanceSchedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StandardMaintenanceSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "StandardMaintenanceSchedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StandardMaintenanceSchedules",
                table: "StandardMaintenanceSchedules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StandardMaintenanceSchedules_VehicleModelId_ConsumableItemId",
                table: "StandardMaintenanceSchedules",
                columns: new[] { "VehicleModelId", "ConsumableItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StandardMaintenanceSchedules",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropIndex(
                name: "IX_StandardMaintenanceSchedules_VehicleModelId_ConsumableItemId",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StandardMaintenanceSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VehicleTypes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "VehicleTypeBrands",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StandardMaintenanceSchedules",
                table: "StandardMaintenanceSchedules",
                columns: new[] { "VehicleModelId", "ConsumableItemId" });
        }
    }
}
