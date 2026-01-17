using System;
using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
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
    private readonly INotificationChannel _notificationChannel;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IUnitOfWork unitOfWork, INotificationChannel notificationChannel)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _notificationChannel = notificationChannel;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing UserRegisteredEvent - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);

        try
        {
            if (!ValidateMessage(message))
            {
                _logger.LogWarning("Invalid UserRegisteredEvent - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
                return;
            }

            await CreateUserPreferenceAsync(message);

            var templateCode = "WELCOME_USER";
            var notificationTemplate = await _unitOfWork.NotificationTemplates.FindOneAsync(nt => nt.Code == templateCode && nt.IsActive);
            if (notificationTemplate == null)
            {
                await SendFallbackWelcomeAsync(message);
                _logger.LogWarning("Notification template not found: {TemplateCode}", templateCode);
                return;
            }

            await SendWelcomeMessageAsync(message, notificationTemplate);
            _logger.LogInformation("Welcome message sent successfully - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserRegisteredEvent - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
            throw;
        }
    }

    private async Task SendWelcomeMessageAsync(UserRegisteredEvent message, NotificationTemplate notificationTemplate)
    {
        var messageContent = ReplaceTemplatePlaceholders(notificationTemplate.MessageTemplate, message);
        var titleContent = ReplaceTemplatePlaceholders(notificationTemplate.TitleTemplate, message);
        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = Guid.NewGuid(),
            RecipientPhone = message.PhoneNumber,
            Title = titleContent,
            Message = messageContent,
            NotificationType = notificationTemplate.NotificationType,
            TemplateParameters = new Dictionary<string, string>
            {
                { "UserName", message.FullName },
                { "RegistrationDate", message.RegistrationDate.ToString("dd/MM/yyyy") }
            },
            Metadata = new Dictionary<string, object>
            {
                { "UserId", message.UserId },
            }
        };
        await _notificationChannel.SendAsync(deliveryContext);
        _logger.LogInformation("Welcome message sent successfully - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
    }

    private string ReplaceTemplatePlaceholders(string messageTemplate, UserRegisteredEvent message)
    {
        if (string.IsNullOrEmpty(messageTemplate))
        {
            return string.Empty;
        }
        return messageTemplate
            .Replace("{UserName}", message.FullName)
            .Replace("{RegistrationDate}", message.RegistrationDate.ToString("dd/MM/yyyy"));
    }

    private async Task SendFallbackWelcomeAsync(UserRegisteredEvent message)
    {
        _logger.LogInformation("Sending fallback welcome message - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
        var welcomeMessage = "Chao mung den voi Verender! Cam on ban da dang ky Verender vao ngay {RegistrationDate}. Chung toi rat vui duoc phuc vu ban!";
        var title = "Chao mung den voi Verender!";
        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = Guid.NewGuid(),
            RecipientPhone = message.PhoneNumber!,
            Title = title,
            Message = welcomeMessage,
            NotificationType = NotificationType.System,
            TemplateParameters = new Dictionary<string, string>
            {
                { "RegistrationDate", message.RegistrationDate.ToString("dd/MM/yyyy") }
            },
            Metadata = new Dictionary<string, object>
            {
                { "UserId", message.UserId },
            }
        };
        _logger.LogInformation("Fallback welcome message sent successfully - UserId: {UserId}, Phone: {Phone}", message.UserId, message.PhoneNumber);
        await _notificationChannel.SendAsync(deliveryContext);
    }

    private async Task CreateUserPreferenceAsync(UserRegisteredEvent message)
    {
        var existingUserPreference = await _unitOfWork.NotificationPreferences.FindOneAsync(np => np.UserId == message.UserId);
        if (existingUserPreference != null)
        {
            _logger.LogWarning("User preference already exists - UserId: {UserId}", message.UserId);
            return;
        }

        var userPreference = new NotificationPreference
        {
            UserId = message.UserId,
            PhoneNumber = message.PhoneNumber,
            PhoneNumberVerified = message.PhoneNumberVerified,
            Email = message.Email,
            EmailVerified = message.EmailVerified
        };
        await _unitOfWork.NotificationPreferences.AddAsync(userPreference);
        await _unitOfWork.SaveChangesAsync();


    }

    private bool ValidateMessage(UserRegisteredEvent message)
    {
        if (string.IsNullOrEmpty(message.PhoneNumber))
        {
            _logger.LogWarning("Phone number is empty - UserId: {UserId}", message.UserId);
            return false;
        }
        if (string.IsNullOrEmpty(message.FullName))
        {
            _logger.LogWarning("Full name is empty - UserId: {UserId}", message.UserId);
            return false;
        }
        if (string.IsNullOrEmpty(message.Email))
        {
            _logger.LogWarning("Email is empty - UserId: {UserId}", message.UserId);
            return false;
        }
        if (message.RegistrationDate == DateTime.MinValue)
        {
            _logger.LogWarning("Registration date is empty - UserId: {UserId}", message.UserId);
            return false;
        }
        return true;
    }
}
