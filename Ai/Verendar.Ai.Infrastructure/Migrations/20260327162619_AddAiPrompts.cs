using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Ai.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPrompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiPromptHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AiPromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPromptHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiPromptHistories_AiPrompts_AiPromptId",
                        column: x => x.AiPromptId,
                        principalTable: "AiPrompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptHistories_AiPromptId_VersionNumber",
                table: "AiPromptHistories",
                columns: new[] { "AiPromptId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPrompts_Operation",
                table: "AiPrompts",
                column: "Operation",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiPromptHistories");

            migrationBuilder.DropTable(
                name: "AiPrompts");
        }
    }
}
