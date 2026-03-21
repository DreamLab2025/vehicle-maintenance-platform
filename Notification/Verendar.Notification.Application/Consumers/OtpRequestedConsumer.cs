using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class OtpRequestedConsumer(
        ILogger<OtpRequestedConsumer> logger,
        IEmailNotificationService emailNotificationService) : IConsumer<OtpRequestedEvent>
    {
        private readonly ILogger<OtpRequestedConsumer> _logger = logger;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;

        public async Task Consume(ConsumeContext<OtpRequestedEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogDebug("Processing OTP email request - MessageId: {MessageId}, UserId: {UserId}, Email: {Email}",
                messageId, message.UserId, MaskEmail(message.TargetValue));

            try
            {
                if (!ValidateMessage(message))
                {
                    _logger.LogWarning("Invalid OTP request message - MessageId: {MessageId}, UserId: {UserId}",
                        messageId, message.UserId);
                    return;
                }

                var success = await _emailNotificationService.SendOtpEmailAsync(message);

                if (success)
                {
                    _logger.LogInformation(
                        "OTP Email processed successfully - MessageId: {MessageId}, UserId: {UserId}, Target: {Target}",
                        messageId, message.UserId, MaskEmail(message.TargetValue));
                }
                else
                {
                    _logger.LogWarning("OTP email send failed - MessageId: {MessageId}, UserId: {UserId}",
                        messageId, message.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }

        private static string? MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return "N/A";
            if (!email.Contains("@")) return "****";
            var parts = email.Split('@');
            if (parts.Length != 2) return "****";
            var username = parts[0];
            var domain = parts[1];
            if (username.Length <= 2) return $"**@{domain}";
            return $"{username[..2]}***@{domain}";
        }

        private static bool ValidateMessage(OtpRequestedEvent message)
        {
            if (message.Type != OtpType.Email) return false;
            if (string.IsNullOrEmpty(message.TargetValue)) return false;
            if (message.ExpiryTime < DateTime.UtcNow) return false;

            return message.TargetValue.Contains("@", StringComparison.Ordinal) &&
                   message.TargetValue.Contains(".", StringComparison.Ordinal);
        }
    }
}
