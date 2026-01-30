using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartCategoryIdentificationAndConsequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsequencesIfNotHandled",
                table: "PartCategories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationSigns",
                table: "PartCategories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Mài mòn nhanh, kẹt piston; hỏng động cơ; chi phí sửa chữa rất cao hoặc phải thay máy.", "Dầu đen, nhớt nhớt; động cơ nóng bất thường; đèn báo dầu sáng; tiếng kêu kim loại từ động cơ." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000002"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Trượt, mất lái khi phanh/trời mưa; nổ lốp khi chạy tốc độ cao; tai nạn nghiêm trọng.", "Gai lốp mòn dưới 1.6mm; nứt rạn; phồng lốp; lốp non hơi thường xuyên; rung lái khi chạy." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000003"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Chết máy giữa đường; hỏng bình; ảnh hưởng bộ sạc và thiết bị điện; không khởi động được.", "Khởi động yếu hoặc không nổ; đèn pha mờ; ắc quy phồng, rỉ nước; xe để vài ngày là hết điện." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000004"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Mất phanh; đĩa phanh hỏng theo; va chạm, tai nạn do không dừng kịp.", "Tiếng kêu ken két khi phanh; phanh không ăn; tay phanh/ pedal bị trễ; đĩa phanh xước, rỗ." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000005-0000-0000-0000-000000000005"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Chết máy; đánh lửa kém gây cháy không hết nhiên liệu, hỏng cat; hao xăng, giảm công suất.", "Khó nổ, giật khi tăng ga; tốn xăng; động cơ rung; bugi đen, dính dầu hoặc cháy trắng đầu điện cực." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000006-0000-0000-0000-000000000006"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Bụi cát vào buồng đốt, mài mòn xy-lanh; giảm công suất; hỏng bugi, hao xăng lâu dài.", "Xe yếu, không bốc; tốn xăng; lọc gió bẩn, rách hoặc ẩm mốc; động cơ nổ không đều." });

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000007-0000-0000-0000-000000000007"),
                columns: new[] { "ConsequencesIfNotHandled", "IdentificationSigns" },
                values: new object[] { "Đứt xích khi chạy, bó bánh; hỏng nhông, moay-ơ; nguy cơ té xe, hỏng hộp số.", "Xích kêu lạch cạch; xích trùng dù đã chỉnh; răng nhông mòn vẹt; xích rỉ, khô dầu." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsequencesIfNotHandled",
                table: "PartCategories");

            migrationBuilder.DropColumn(
                name: "IdentificationSigns",
                table: "PartCategories");
        }
    }
}
