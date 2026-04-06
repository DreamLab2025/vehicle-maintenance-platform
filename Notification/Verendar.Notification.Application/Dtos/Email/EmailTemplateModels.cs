namespace Verendar.Notification.Application.Dtos.Email;

public class OtpEmailModel
{
    public string UserName { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public DateTime ExpiryTime { get; set; }
}

public class NotificationEmailModel
{
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? CtaUrl { get; set; }
    public string? CtaText { get; set; }
}

public class MemberAccountCreatedEmailModel
{
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TempPassword { get; set; } = string.Empty;
    public string? CtaUrl { get; set; }
    public string? CtaText { get; set; }
}
