using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.ExternalServices.Resend;

namespace Verendar.Notification.Infrastructure.Services;

public class EmailNotificationService(
    EmailChannel emailChannel,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    private readonly EmailChannel _emailChannel = emailChannel;
    private readonly ILogger<EmailNotificationService> _logger = logger;

    public Task<bool> SendOtpEmailAsync(
        string email,
        string otpCode,
        DateTime expiresAt,
        string otpType,
        CancellationToken cancellationToken = default)
    {
        var expiryMinutes = CalculateExpiryMinutes(expiresAt);
        var title = NotificationConstants.Titles.Otp;
        var messageContent =
            $"Mã xác thực OTP Verendar của bạn là: {otpCode}. Hiệu lực {Math.Ceiling(expiryMinutes)} phút. Không chia sẻ mã này.";

        var ctx = new NotificationDeliveryContext
        {
            NotificationId = Guid.Empty,
            RecipientEmail = email,
            Title = title,
            Message = messageContent,
            NotificationType = NotificationType.System,
            TemplateModel = new OtpEmailModel
            {
                UserName = email,
                OtpCode = otpCode,
                ExpiryMinutes = (int)Math.Ceiling(expiryMinutes),
                ExpiryTime = expiresAt,
                OtpType = otpType
            },
            Metadata = new Dictionary<string, object>
            {
                { NotificationConstants.MetadataKeys.TemplateKey, NotificationConstants.TemplateKeys.Otp }
            }
        };

        return SendEmailOnlyAsync(ctx, cancellationToken);
    }

    public Task<bool> SendMemberAccountCreatedEmailAsync(
        string email,
        string displayName,
        string tempPassword,
        string role,
        string loginUrl,
        CancellationToken cancellationToken = default)
    {
        var title = NotificationConstants.Titles.MemberAccountCreated;
        var message =
            $"Tài khoản Garage của <strong>{displayName}</strong> đã được tạo. "
            + $"Vai trò: <strong>{role}</strong>. Mật khẩu tạm: <strong>{tempPassword}</strong>. "
            + "Vui lòng đổi mật khẩu sau lần đăng nhập đầu tiên.";

        return SendNotificationEmailAsync(
            email,
            title,
            message,
            loginUrl,
            NotificationConstants.ConsumerCopy.LoginCta,
            displayName,
            cancellationToken);
    }

    public Task<bool> SendNotificationEmailAsync(
        string email,
        string title,
        string message,
        string ctaUrl,
        string ctaText,
        string? userName,
        CancellationToken cancellationToken = default)
    {
        var ctx = new NotificationDeliveryContext
        {
            NotificationId = Guid.Empty,
            RecipientEmail = email,
            Title = title,
            Message = message,
            NotificationType = NotificationType.User,
            TemplateModel = new NotificationEmailModel
            {
                UserName = userName ?? email,
                Title = title,
                Message = message,
                ActionUrl = ctaUrl,
                ActionText = ctaText
            },
            Metadata = new Dictionary<string, object>
            {
                { NotificationConstants.MetadataKeys.TemplateKey, NotificationConstants.TemplateKeys.Notification }
            }
        };

        return SendEmailOnlyAsync(ctx, cancellationToken);
    }

    private async Task<bool> SendEmailOnlyAsync(NotificationDeliveryContext context, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _emailChannel.SendAsync(context);
            if (!result.IsSuccess)
                _logger.LogWarning("Email send failed: {Error}", result.ErrorMessage);
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send threw for recipient {Recipient}", context.RecipientEmail);
            return false;
        }
    }

    private static double CalculateExpiryMinutes(DateTime expiryTime)
    {
        var timeSpan = expiryTime - DateTime.UtcNow;
        return timeSpan.TotalMinutes > 0 ? timeSpan.TotalMinutes : 0;
    }
}
