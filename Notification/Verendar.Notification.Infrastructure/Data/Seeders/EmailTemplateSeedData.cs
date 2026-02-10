using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.Seeders
{
    public static class EmailTemplateSeedData
    {
        public static readonly IReadOnlyList<EmailTemplateSeedItem> Items = new[]
        {
            new EmailTemplateSeedItem(
                Code: "OTP_EMAIL_VERIFICATION",
                TitleTemplate: "Mã xác thực email",
                MessageTemplate: "Mã xác thuc email Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Khong chia se ma nay voi bat ky ai.",
                NotificationType: NotificationType.System),
            new EmailTemplateSeedItem(
                Code: "WELCOME_USER",
                TitleTemplate: "Chào mừng đến với Verendar!",
                MessageTemplate: "Xin chao {UserName}! Cam on ban da dang ky Verendar. Chung toi rat vui duoc phuc vu ban!",
                NotificationType: NotificationType.Welcome),
            new EmailTemplateSeedItem(
                Code: "OTP_PASSWORD_RESET",
                TitleTemplate: "Đặt lại mật khẩu",
                MessageTemplate: "Ma dat lai mat khau Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Neu ban khong yeu cau, vui long bo qua email nay.",
                NotificationType: NotificationType.System),
            // Thông báo reminder khi đạt mức (trùng 3 mức xe test: Critical, High, Medium)
            new EmailTemplateSeedItem(
                Code: "MAINTENANCE_REMINDER_Critical",
                TitleTemplate: "Khẩn cấp: Cần thay linh kiện",
                MessageTemplate: "Xe cua ban co linh kien da den muc khan cap can thay the. Ban se nhan duoc email nhac nho hang ngay cho den khi ban cap nhat da thay linh kien.\n\nCac linh kien can chu y:\n{PartList}\n\nVui long vao app cap nhat sau khi thay linh kien de dung nhac nho.",
                NotificationType: NotificationType.User),
            new EmailTemplateSeedItem(
                Code: "MAINTENANCE_REMINDER_HIGH",
                TitleTemplate: "Nhắc nhở bảo dưỡng (High)",
                MessageTemplate: "Xe cua ban co linh kien can chu y bao duong/thay the:\n\n{PartList}",
                NotificationType: NotificationType.User),
            new EmailTemplateSeedItem(
                Code: "MAINTENANCE_REMINDER_MEDIUM",
                TitleTemplate: "Nhắc nhở bảo dưỡng (Medium)",
                MessageTemplate: "Xe cua ban co linh kien can chu y bao duong/thay the:\n\n{PartList}",
                NotificationType: NotificationType.User),
        };
    }

    public record EmailTemplateSeedItem(
        string Code,
        string TitleTemplate,
        string MessageTemplate,
        NotificationType NotificationType);
}
