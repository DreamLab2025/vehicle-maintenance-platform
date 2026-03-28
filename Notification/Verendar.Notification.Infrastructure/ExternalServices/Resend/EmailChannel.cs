using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend
{
    public class EmailChannel(IResendEmailService emailService, ILogger<EmailChannel> logger) : INotificationChannel
    {
        private readonly IResendEmailService _emailService = emailService;
        private readonly ILogger<EmailChannel> _logger = logger;

        public NotificationChannel ChannelType => NotificationChannel.EMAIL;

        public async Task<ChannelDeliveryResult> SendAsync(NotificationDeliveryContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(context.RecipientEmail))
                {
                    _logger.LogError("Recipient email is required");
                    return ChannelDeliveryResult.Failed("Email người nhận không hợp lệ");
                }

                var recipientEmail = context.RecipientEmail;

                _logger.LogInformation("Sending email to {Email}, NotificationId: {NotificationId}",
                    recipientEmail, context.NotificationId);

                var templateKey = context.Metadata?.GetValueOrDefault("TemplateKey")?.ToString() ?? "Notification";
                var model = context.TemplateModel ?? new NotificationEmailModel
                {
                    Title = context.Title,
                    Message = context.Message,
                    UserName = recipientEmail
                };

                var result = await _emailService.SendTemplatedEmailAsync(
                    recipientEmail,
                    templateKey,
                    context.Title,
                    model,
                    cancellationToken: default);

                if (result.IsSuccess)
                    return ChannelDeliveryResult.Success(result.MessageId);

                return ChannelDeliveryResult.Failed(
                    $"Resend Error: {result.ErrorMessage}",
                    shouldRetry: ShouldRetry(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email delivery failed for notification {NotificationId}", context.NotificationId);
                return ChannelDeliveryResult.Failed(ex.Message, shouldRetry: true);
            }
        }

        private static bool ShouldRetry(string? errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return true;

            var nonRetryableErrors = new[] { "invalid email", "validation", "unauthorized", "forbidden", "not found" };
            return !nonRetryableErrors.Any(e => errorMessage.Contains(e, StringComparison.OrdinalIgnoreCase));
        }
    }
}
