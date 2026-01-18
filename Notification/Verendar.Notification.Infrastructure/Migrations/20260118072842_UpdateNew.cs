using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Verendar.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.AddColumn<string>(
                name: "ExternalTemplateId",
                table: "NotificationTemplateChannels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.InsertData(
                table: "NotificationTemplateChannels",
                columns: new[] { "Id", "Channel", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "ExternalTemplateId", "IsEnabled", "NotificationTemplateId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("10000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000002"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("10000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000003"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("10000000-0000-0000-0000-000000000004"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000004"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("10000000-0000-0000-0000-000000000005"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000005"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000006"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000007"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("30000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000008"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("30000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("90000000-0000-0000-0000-000000000009"), 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, new Guid("30000000-0000-0000-0000-000000000003"), null, null }
                });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma xac thuc: {OTP}. Het han sau {ExpiryMinutes}p.", "Ma xac thuc SDT" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "MessageTemplate",
                value: "Ma reset pass: {OTP}. Het han sau {ExpiryMinutes}p.");

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma 2FA: {OTP}.", "Ma 2FA" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma OTP: {OTP}.", "Ma OTP" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Xin chao {UserName} den voi Verender!", "Chao mung!" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "SDT {PhoneNumber} da xac thuc.", "SDT da xac thuc" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Mat khau da doi luc {ChangeTime}.", "Doi mat khau" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ly do: {Reason}.", "Tai khoan bi khoa" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Tai khoan da duoc mo.", "Mo khoa tai khoan" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "NotificationTemplateChannels",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000009"));

            migrationBuilder.DropColumn(
                name: "ExternalTemplateId",
                table: "NotificationTemplateChannels");

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma xac thuc Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Khong chia se ma nay voi bat ky ai.", "Ma xac thuc so dien thoai" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "MessageTemplate",
                value: "Ma dat lai mat khau Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Neu ban khong yeu cau dat lai mat khau, vui long bo qua tin nhan nay.");

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma xac thuc hai buoc Verender: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Khong chia se ma nay.", "Ma xac thuc hai buoc" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Ma xac thuc Verender: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Vui long khong chia se ma nay voi bat ky ai.", "Ma xac thuc OTP" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Xin chao {UserName}! Cam on ban da dang ky Verender vao ngay {RegistrationDate}. Chung toi rat vui duoc phuc vu ban!", "Chao mung den voi Verender!" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "So dien thoai {PhoneNumber} cua ban da duoc xac thuc thanh cong. Tai khoan Verender cua ban da san sang su dung!", "So dien thoai da duoc xac thuc" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Mat khau tai khoan Verender cua ban da duoc thay doi vao {ChangeTime}. Neu ban khong thuc hien thay doi nay, vui long lien he ho tro ngay lap tuc.", "Mat khau da duoc thay doi" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Tai khoan Verender cua ban da bi khoa do {Reason}. Vui long lien he ho tro de duoc tro giup.", "Tai khoan bi tam khoa" });

            migrationBuilder.UpdateData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                columns: new[] { "MessageTemplate", "TitleTemplate" },
                values: new object[] { "Tai khoan Verender cua ban da duoc mo khoa thanh cong. Ban co the dang nhap binh thuong.", "Tai khoan da duoc mo khoa" });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DefaultPriority", "DeletedAt", "DeletedBy", "IsActive", "MessageTemplate", "NotificationType", "TitleTemplate", "UpdatedAt", "UpdatedBy", "VariablesJson" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000002"), "OTP_EMAIL_VERIFICATION", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), 2, null, null, true, "Ma xac thuc email Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut.", 1, "Ma xac thuc email", null, null, null });
        }
    }
}
