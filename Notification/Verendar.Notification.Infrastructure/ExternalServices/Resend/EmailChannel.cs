using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend
{
    public class EmailChannel : INotificationChannel
    {
        private readonly IResendEmailService _emailService;
        private readonly ILogger<EmailChannel> _logger;

        public EmailChannel(IResendEmailService emailService, ILogger<EmailChannel> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

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

                _logger.LogInformation("Sending email to {Email}, NotificationId: {NotificationId}",
                    context.RecipientEmail, context.NotificationId);

                ResendEmailResponse result;

                var templateKey = context.Metadata?.GetValueOrDefault("TemplateKey")?.ToString();
                var useTemplateModel = !string.IsNullOrEmpty(templateKey) && context.TemplateModel != null;
                var useTemplateParams = !string.IsNullOrEmpty(templateKey) &&
                                       (context.TemplateParameters != null && context.TemplateParameters.Any());

                if (useTemplateModel)
                {
                    result = await _emailService.SendTemplatedEmailAsync(
                        context.RecipientEmail!,
                        templateKey!,
                        context.Title,
                        context.TemplateModel,
                        cancellationToken: default);
                }
                else if (useTemplateParams)
                {
                    var model = CreateTemplateModel(context);
                    result = await _emailService.SendTemplatedEmailAsync(
                        context.RecipientEmail!,
                        templateKey!,
                        context.Title,
                        model,
                        cancellationToken: default);
                }
                else
                {
                    // Send plain email using Notification template
                    var notificationModel = new NotificationEmailModel
                    {
                        Title = context.Title,
                        Message = context.Message,
                        UserName = context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User",
                        ActionUrl = context.Metadata?.GetValueOrDefault("ActionUrl")?.ToString()
                    };

                    result = await _emailService.SendTemplatedEmailAsync(
                        context.RecipientEmail!,
                        "Notification",
                        context.Title,
                        notificationModel,
                        cancellationToken: default);
                }

                if (result.IsSuccess)
                {
                    return ChannelDeliveryResult.Success(result.MessageId);
                }

                return ChannelDeliveryResult.Failed(
                    $"Resend Error: {result.ErrorMessage}",
                    shouldRetry: ShouldRetry(result.ErrorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email delivery failed for notification {NotificationId}",
                    context.NotificationId);
                return ChannelDeliveryResult.Failed(ex.Message, shouldRetry: true);
            }
        }

        private object CreateTemplateModel(NotificationDeliveryContext context)
        {
            // Create appropriate model based on notification type or template key
            if (context.TemplateParameters == null || !context.TemplateParameters.Any())
            {
                return new NotificationEmailModel
                {
                    Title = context.Title,
                    Message = context.Message,
                    UserName = context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User",
                    ActionUrl = context.Metadata?.GetValueOrDefault("ActionUrl")?.ToString()
                };
            }

            // Try to map to specific models based on template parameters
            if (context.TemplateParameters.ContainsKey("OTP") || context.TemplateParameters.ContainsKey("OtpCode"))
            {
                return new OtpEmailModel
                {
                    OtpCode = context.TemplateParameters.GetValueOrDefault("OTP") ??
                             context.TemplateParameters.GetValueOrDefault("OtpCode") ?? string.Empty,
                    ExpiryMinutes = int.TryParse(context.TemplateParameters.GetValueOrDefault("ExpiryMinutes"), out var exp) ? exp : 10,
                    ExpiryTime = DateTime.TryParse(context.TemplateParameters.GetValueOrDefault("ExpiryTime"), out var expTime)
                        ? expTime : DateTime.UtcNow.AddMinutes(10),
                    OtpType = context.TemplateParameters.GetValueOrDefault("Type") ?? "Verification",
                    UserName = context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User"
                };
            }

            if (context.TemplateParameters.ContainsKey("FullName") || context.Metadata?.ContainsKey("UserName") == true)
            {
                return new WelcomeEmailModel
                {
                    FullName = context.TemplateParameters.GetValueOrDefault("FullName") ??
                              context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User",
                    RegistrationDate = DateTime.TryParse(context.Metadata?.GetValueOrDefault("RegistrationDate")?.ToString(), out var regDate)
                        ? regDate : DateTime.UtcNow,
                    UserName = context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User"
                };
            }

            // Default to generic notification model
            return new NotificationEmailModel
            {
                Title = context.Title,
                Message = context.Message,
                UserName = context.Metadata?.GetValueOrDefault("UserName")?.ToString() ?? "User",
                ActionUrl = context.Metadata?.GetValueOrDefault("ActionUrl")?.ToString()
            };
        }

        private static bool ShouldRetry(string? errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return true;

            // Don't retry for validation errors
            var nonRetryableErrors = new[]
            {
                "invalid email",
                "validation",
                "unauthorized",
                "forbidden",
                "not found"
            };

            return !nonRetryableErrors.Any(error =>
                errorMessage.Contains(error, StringComparison.OrdinalIgnoreCase));
        }
    }
}
