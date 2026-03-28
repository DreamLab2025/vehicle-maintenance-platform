using Verendar.Notification.Application.Dtos.Email;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface IEmailNotificationService
{
    Task<bool> SendOtpEmailAsync(
        string email,
        string otpCode,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    Task<bool> SendMemberAccountCreatedEmailAsync(
        string recipientEmail,
        MemberAccountCreatedEmailModel model,
        CancellationToken cancellationToken = default);

    Task<bool> SendNotificationEmailAsync(
        string email,
        string title,
        string message,
        string ctaUrl,
        string ctaText,
        CancellationToken cancellationToken = default);
}
