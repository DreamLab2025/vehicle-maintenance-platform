using System;
using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChannelFactory _channelFactory;

    public UserRegisteredConsumer(
        ILogger<UserRegisteredConsumer> logger,
        IUnitOfWork unitOfWork,
        IChannelFactory channelFactory)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing UserRegisteredEvent - UserId: {UserId}, Phone: {Phone}",
            message.UserId, message.PhoneNumber);

        try
        {
            if (!ValidateMessage(message))
            {
                _logger.LogWarning("Invalid UserRegisteredEvent - UserId: {UserId}", message.UserId);
                return;
            }

            // Tạo preference trước
            await CreateUserPreferenceAsync(message);

            // Load template
            var templateCode = "WELCOME_USER";
            var notificationTemplate = await _unitOfWork.NotificationTemplates
                .FindOneAsync(nt => nt.Code == templateCode && nt.IsActive);

            if (notificationTemplate == null)
            {
                _logger.LogWarning("Notification template not found: {TemplateCode}", templateCode);
                await SendFallbackWelcomeAsync(message);
                return;
            }

            await SendWelcomeMessageAsync(message, notificationTemplate);

            _logger.LogInformation("Welcome message processed successfully - UserId: {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserRegisteredEvent - UserId: {UserId}", message.UserId);
            throw;
        }
    }

    private async Task SendWelcomeMessageAsync(UserRegisteredEvent message, NotificationTemplate notificationTemplate)
    {
        var messageContent = ReplaceTemplatePlaceholders(notificationTemplate.MessageTemplate, message);
        var titleContent = ReplaceTemplatePlaceholders(notificationTemplate.TitleTemplate, message);

        // Welcome message luôn gửi qua SMS
        var targetChannel = NotificationChannel.SMS;

        // Tạo & Lưu DB trước
        var notification = message.UserRegisteredToNotificationEntity(
            titleContent,
            messageContent,
            notificationTemplate.NotificationType
        );
        var delivery = notification.CreateDelivery(message.PhoneNumber!, targetChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        // Dùng notification.Id từ DB
        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientPhone = message.PhoneNumber!,
            Title = titleContent,
            Message = messageContent,
            NotificationType = notificationTemplate.NotificationType,
            Metadata = new Dictionary<string, object>
            {
                { "UserId", message.UserId },
                { "UserName", message.FullName },
                { "RegistrationDate", message.RegistrationDate.ToString("dd/MM/yyyy") }
            }
        };

        // Gửi & Update Status
        await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
    }

    private async Task SendFallbackWelcomeAsync(UserRegisteredEvent message)
    {
        _logger.LogInformation("Sending fallback welcome message - UserId: {UserId}", message.UserId);

        var welcomeMessage = $"Chao mung {message.FullName} den voi Verender! " +
                            $"Cam on ban da dang ky vao ngay {message.RegistrationDate:dd/MM/yyyy}. " +
                            $"Chung toi rat vui duoc phuc vu ban!";
        var title = "Chao mung den voi Verender!";

        var targetChannel = NotificationChannel.SMS;

        // Tạo & Lưu DB
        var notification = message.UserRegisteredToNotificationEntity(
            title,
            welcomeMessage,
            NotificationType.System,
            true
        );
        var delivery = notification.CreateDelivery(message.PhoneNumber!, targetChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientPhone = message.PhoneNumber!,
            Title = title,
            Message = welcomeMessage,
            NotificationType = NotificationType.System,
            Metadata = new Dictionary<string, object>
            {
                { "UserId", message.UserId },
                { "IsFallback", true }
            }
        };

        await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
    }

    private async Task ExecuteSendAsync(
        NotificationChannel channel,
        NotificationDeliveryContext context,
        Domain.Entities.Notification notification,
        NotificationDelivery delivery)
    {
        try
        {
            var channelService = _channelFactory.GetChannel(channel);
            var result = await channelService.SendAsync(context);

            if (result.IsSuccess)
            {
                notification.Status = NotificationStatus.Sent;
                delivery.Status = NotificationStatus.Sent;
                delivery.SentAt = DateTime.UtcNow;
                delivery.DeliveredAt = DateTime.UtcNow;
                delivery.ExternalId = result.ExternalId;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                delivery.Status = NotificationStatus.Failed;
                delivery.ErrorMessage = result.ErrorMessage;
                delivery.RetryCount++;

                if (result.ShouldRetry) throw new Exception(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            delivery.Status = NotificationStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private string ReplaceTemplatePlaceholders(string template, UserRegisteredEvent message)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        return template
            .Replace("{UserName}", message.FullName)
            .Replace("{FullName}", message.FullName)
            .Replace("{RegistrationDate}", message.RegistrationDate.ToString("dd/MM/yyyy"));
    }

    private Task CreateUserPreferenceAsync(UserRegisteredEvent message) => _unitOfWork.NotificationPreferences.AddAsync(message.UserRegisteredToPreferenceEntity());

    private bool ValidateMessage(UserRegisteredEvent message)
    {
        if (string.IsNullOrEmpty(message.PhoneNumber)) return false;
        if (string.IsNullOrEmpty(message.FullName)) return false;
        if (message.RegistrationDate == DateTime.MinValue) return false;
        return true;
    }
}