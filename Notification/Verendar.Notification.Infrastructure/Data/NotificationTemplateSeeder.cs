using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.Data
{
    public static class NotificationTemplateSeeder
    {
        public static List<NotificationTemplate> GetNotificationTemplates()
        {
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return new List<NotificationTemplate>
            {                
                // Phone Number Verification (Registration/Login)
                new NotificationTemplate
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    Code = "OTP_PHONE_VERIFICATION",
                    TitleTemplate = "Ma xac thuc so dien thoai",
                    MessageTemplate = "Ma xac thuc Verender cua ban la: {OTP}. " +
                                      "Ma co hieu luc trong {ExpiryMinutes} phut. " +
                                      "Khong chia se ma nay voi bat ky ai.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Email Verification
                new NotificationTemplate
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    Code = "OTP_EMAIL_VERIFICATION",
                    TitleTemplate = "Ma xac thuc email",
                    MessageTemplate = "Ma xac thuc email Verender cua ban la: {OTP}. " +
                                      "Ma co hieu luc trong {ExpiryMinutes} phut.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Password Reset
                new NotificationTemplate
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    Code = "OTP_PASSWORD_RESET",
                    TitleTemplate = "Ma dat lai mat khau",
                    MessageTemplate = "Ma dat lai mat khau Verender cua ban la: {OTP}. " +
                                      "Ma co hieu luc trong {ExpiryMinutes} phut. " +
                                      "Neu ban khong yeu cau dat lai mat khau, vui long bo qua tin nhan nay.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Two Factor Authentication
                new NotificationTemplate
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    Code = "OTP_TWO_FACTOR",
                    TitleTemplate = "Ma xac thuc hai buoc",
                    MessageTemplate = "Ma xac thuc hai buoc Verender: {OTP}. " +
                                      "Ma co hieu luc trong {ExpiryMinutes} phut. " +
                                      "Khong chia se ma nay.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Default OTP Template
                new NotificationTemplate
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                    Code = "OTP_REQUESTED",
                    TitleTemplate = "Ma xac thuc OTP",
                    MessageTemplate = "Ma xac thuc Verender: {OTP}. " +
                                      "Ma co hieu luc trong {ExpiryMinutes} phut. " +
                                      "Vui long khong chia se ma nay voi bat ky ai.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // --- Welcome Templates ---

                // User Welcome
                new NotificationTemplate
                {
                    Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    Code = "WELCOME_USER",
                    TitleTemplate = "Chao mung den voi Verender!",
                    MessageTemplate = "Xin chao {UserName}! " +
                                      "Cam on ban da dang ky Verender vao ngay {RegistrationDate}. " +
                                      "Chung toi rat vui duoc phuc vu ban!",
                    NotificationType = NotificationType.User,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Phone Verified
                new NotificationTemplate
                {
                    Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    Code = "PHONE_VERIFIED",
                    TitleTemplate = "So dien thoai da duoc xac thuc",
                    MessageTemplate = "So dien thoai {PhoneNumber} cua ban da duoc xac thuc thanh cong. " +
                                      "Tai khoan Verender cua ban da san sang su dung!",
                    NotificationType = NotificationType.User,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // --- System Templates ---

                // Password Changed
                new NotificationTemplate
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    Code = "PASSWORD_CHANGED",
                    TitleTemplate = "Mat khau da duoc thay doi",
                    MessageTemplate = "Mat khau tai khoan Verender cua ban da duoc thay doi vao {ChangeTime}. " +
                                      "Neu ban khong thuc hien thay doi nay, vui long lien he ho tro ngay lap tuc.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Account Locked
                new NotificationTemplate
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                    Code = "ACCOUNT_LOCKED",
                    TitleTemplate = "Tai khoan bi tam khoa",
                    MessageTemplate = "Tai khoan Verender cua ban da bi khoa do {Reason}. " +
                                      "Vui long lien he ho tro de duoc tro giup.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                },

                // Account Unlocked
                new NotificationTemplate
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                    Code = "ACCOUNT_UNLOCKED",
                    TitleTemplate = "Tai khoan da duoc mo khoa",
                    MessageTemplate = "Tai khoan Verender cua ban da duoc mo khoa thanh cong. " +
                                      "Ban co the dang nhap binh thuong.",
                    NotificationType = NotificationType.System,
                    IsActive = true,
                    CreatedAt = fixedDate
                }
            };
        }
    }
}