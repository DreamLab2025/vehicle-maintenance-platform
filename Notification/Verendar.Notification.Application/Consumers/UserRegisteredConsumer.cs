using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class UserRegisteredConsumer(
        ILogger<UserRegisteredConsumer> logger,
        IUnitOfWork unitOfWork,
        IChannelFactory channelFactory) : IConsumer<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredConsumer> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IChannelFactory _channelFactory = channelFactory;

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        
            _logger.LogInformation("Processing UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}, Phone: {Phone}",
                messageId, message.UserId, message.PhoneNumber);

            try
            {
                if (!ValidateMessage(message))
                {
                    _logger.LogWarning("Invalid UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}", 
                        messageId, message.UserId);
                    return;
                }

                await CreateUserPreferenceAsync(message);

                var templateCode = "WELCOME_USER";
                var notificationTemplate = await _unitOfWork.NotificationTemplates
                    .FindOneAsync(nt => nt.Code == templateCode && nt.IsActive);

                if (notificationTemplate == null)
                {
                    _logger.LogWarning("Notification template not found: {TemplateCode}", templateCode);
                    await SendFallbackWelcomeAsync(message, messageId);
                    return;
                }

                await SendWelcomeMessageAsync(message, notificationTemplate, messageId);

                _logger.LogInformation("Welcome message processed successfully - MessageId: {MessageId}, UserId: {UserId}", 
                    messageId, message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserRegisteredEvent - MessageId: {MessageId}, UserId: {UserId}", 
                    messageId, message.UserId);
            }
        }

        private async Task SendWelcomeMessageAsync(UserRegisteredEvent message, NotificationTemplate notificationTemplate, string? messageId = null)
        {
            var messageContent = ReplaceTemplatePlaceholders(notificationTemplate.MessageTemplate, message);
            var titleContent = ReplaceTemplatePlaceholders(notificationTemplate.TitleTemplate, message);

            var targetChannel = NotificationChannel.SMS;

            var notification = message.UserRegisteredToNotificationEntity(
                titleContent,
                messageContent,
                notificationTemplate.NotificationType
            );
            var delivery = notification.CreateDelivery(message.PhoneNumber!, targetChannel);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync();

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

            try
            {
                await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome message after notification created - NotificationId: {NotificationId}, MessageId: {MessageId}",
                    notification.Id, messageId);
            }
        }

        private async Task SendFallbackWelcomeAsync(UserRegisteredEvent message, string? messageId = null)
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

            try
            {
                await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
            }
            catch (Exception ex)
            {
                // Notification đã được lưu vào DB, không throw để tránh retry
                _logger.LogError(ex, "Failed to send fallback welcome message after notification created - NotificationId: {NotificationId}, MessageId: {MessageId}",
                    notification.Id, messageId);
            }
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
                // Không throw exception ở đây - đã được handle ở method gọi
                _logger.LogError(ex, "Error in ExecuteSendAsync - NotificationId: {NotificationId}", notification.Id);
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
}