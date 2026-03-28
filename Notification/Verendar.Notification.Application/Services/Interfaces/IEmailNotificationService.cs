namespace Verendar.Notification.Application.Services.Interfaces;

public interface IEmailNotificationService
{
    Task<bool> SendOtpEmailAsync(
        string email,
        string otpCode,
        DateTime expiresAt,
        string otpType,
        CancellationToken cancellationToken = default);

    Task<bool> SendMemberAccountCreatedEmailAsync(
        string email,
        string displayName,
        string tempPassword,
        string role,
        string loginUrl,
        CancellationToken cancellationToken = default);

    Task<bool> SendNotificationEmailAsync(
        string email,
        string title,
        string message,
        string ctaUrl,
        string ctaText,
        string? userName,
        CancellationToken cancellationToken = default);
}
