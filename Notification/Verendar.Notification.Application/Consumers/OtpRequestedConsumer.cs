using System;
using System.Text.RegularExpressions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public partial class OtpRequestedConsumer : IConsumer<OtpRequestedEvent>
{
    [GeneratedRegex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
    [GeneratedRegex("^0[0-9]{9}$")]
    private static partial Regex PhoneNumberRegex();
    private readonly ILogger<OtpRequestedConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationChannel _notificationChannel;
    public OtpRequestedConsumer(ILogger<OtpRequestedConsumer> logger, IUnitOfWork unitOfWork, INotificationChannel notificationChannel)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _notificationChannel = notificationChannel;
    }

    public async Task Consume(ConsumeContext<OtpRequestedEvent> context)
    {
        var message = context.Message;

        _logger.LogDebug("Processing OTP request - UserId: {UserId}, Phone: {Phone}", message.UserId, message.TargetValue);

        try
        {
            if (!ValidateMessage(message))
            {
                _logger.LogWarning(
                    "Invalid OTP request message - UserId: {UserId}",
                    message.UserId);
                return;
            }

            var templateCode = GetTemplateCode(message.Type);
            var notificationTemplate = await _unitOfWork.NotificationTemplates
                .FindOneAsync(nt => nt.Code == templateCode && nt.IsActive);

            if (notificationTemplate == null)
            {
                _logger.LogWarning(
                    "Notification template not found: {TemplateCode} for type: {Type}",
                    templateCode,
                    message.Type);

                await SendFallbackOtpAsync(message);
                return;
            }

            var messageContent = ReplaceTemplatePlaceholders(
                notificationTemplate.MessageTemplate,
                message);

            var titleContent = ReplaceTemplatePlaceholders(
                notificationTemplate.TitleTemplate,
                message);

            var deliveryContext = new NotificationDeliveryContext
            {
                NotificationId = Guid.NewGuid(),
                RecipientPhone = message.TargetValue,
                Title = titleContent,
                Message = messageContent,
                NotificationType = notificationTemplate.NotificationType,
                TemplateParameters = new Dictionary<string, string>
                    {
                        { "OTP", message.Otp },
                        { "ExpiryMinutes", CalculateExpiryMinutes(message.ExpiryTime).ToString() }
                    },
                Metadata = new Dictionary<string, object>
                    {
                        { "UserId", message.UserId },
                        { "Type", message.Type },
                        { "ExpiryTime", message.ExpiryTime },
                        { "EventId", message.EventId }
                    }
            };

            await _notificationChannel.SendAsync(deliveryContext);

            _logger.LogInformation(
                "OTP SMS sent successfully - UserId: {UserId}, Phone: {Phone}, Type: {Type}",
                message.UserId,
                MaskPhoneNumber(message.TargetValue),
                message.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending OTP SMS - UserId: {UserId}, Type: {Type}",
                message.UserId,
                message.Type);

            throw;
        }
    }

    private string? MaskPhoneNumber(string? phoneNumber)
    {
        throw new NotImplementedException();
    }

    private double CalculateExpiryMinutes(DateTime expiryTime)
    {
        var timeSpan = expiryTime - DateTime.UtcNow;
        return timeSpan.TotalMinutes;
    }

    private string ReplaceTemplatePlaceholders(string template, OtpRequestedEvent message)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        var expiryMinutes = CalculateExpiryMinutes(message.ExpiryTime);

        return template
            .Replace("{OTP}", message.Otp)
            .Replace("{Otp}", message.Otp)
            .Replace("{ExpiryMinutes}", expiryMinutes.ToString())
            .Replace("{ExpiryTime}", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy"))
            .Replace("{Type}", message.Type.ToString());
    }

    private async Task SendFallbackOtpAsync(OtpRequestedEvent message)
    {
        _logger.LogInformation(
            "Sending fallback OTP message for UserId: {UserId}",
            message.UserId);

        var expiryMinutes = CalculateExpiryMinutes(message.ExpiryTime);

        var fallbackMessage = $"Ma xac thuc OTP Verender cua ban la: {message.Otp}. " +
                            $"Ma co hieu luc trong {expiryMinutes} phut. " +
                            $"Khong chia se ma nay voi bat ky ai.";

        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = Guid.NewGuid(),
            RecipientPhone = message.TargetValue!,
            Title = "Ma xac thuc OTP",
            Message = fallbackMessage,
            NotificationType = Domain.Enums.NotificationType.System,
            TemplateParameters = new Dictionary<string, string>
                {
                    { "OTP", message.Otp }
                },
            Metadata = new Dictionary<string, object>
                {
                    { "UserId", message.UserId },
                    { "OtpType", message.Type },
                    { "IsFallback", true }
                }
        };

        await _notificationChannel.SendAsync(deliveryContext);
    }

    private static string GetTemplateCode(OtpType type)
    {
        return type switch
        {
            OtpType.PhoneNumber => "OTP_PHONE_VERIFICATION",
            OtpType.Email => "OTP_EMAIL_VERIFICATION",
            _ => "OTP_REQUESTED"
        };
    }

    private bool ValidateMessage(OtpRequestedEvent message)
    {
        if (string.IsNullOrEmpty(message.TargetValue))
        {
            _logger.LogWarning("Target value is empty - UserId: {UserId}", message.UserId);
            return false;
        }

        if (message.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning("Expiry time is in the past - UserId: {UserId}", message.UserId);
            return false;
        }
        if (message.Type == OtpType.PhoneNumber && !PhoneNumberRegex().IsMatch(message.TargetValue))
        {
            _logger.LogWarning("Target value is not a valid phone number - UserId: {UserId}", message.UserId);
            return false;
        }
        if (message.Type == OtpType.Email && !EmailRegex().IsMatch(message.TargetValue))
        {
            _logger.LogWarning("Target value is not a valid email - UserId: {UserId}", message.UserId);
            return false;
        }
        _logger.LogInformation("Message validated successfully - UserId: {UserId}", message.UserId);
        return true;
    }
}
