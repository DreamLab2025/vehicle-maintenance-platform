using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.Seeders;

/// <summary>
/// Định nghĩa các email template seed vào database (thay vì hardcode trong migration).
/// </summary>
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
            NotificationType: NotificationType.User),
        new EmailTemplateSeedItem(
            Code: "OTP_PASSWORD_RESET",
            TitleTemplate: "Đặt lại mật khẩu",
            MessageTemplate: "Ma dat lai mat khau Verender cua ban la: {OTP}. Ma co hieu luc trong {ExpiryMinutes} phut. Neu ban khong yeu cau, vui long bo qua email nay.",
            NotificationType: NotificationType.System),
    };
}

public record EmailTemplateSeedItem(
    string Code,
    string TitleTemplate,
    string MessageTemplate,
    NotificationType NotificationType);
