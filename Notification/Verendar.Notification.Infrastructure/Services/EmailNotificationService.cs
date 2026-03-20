using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Enums;
using Verendar.Vehicle.Contracts.Events;
using NotificationEntity = Verendar.Notification.Domain.Entities.Notification;

namespace Verendar.Notification.Infrastructure.Services
{
    public class EmailNotificationService(
        IUnitOfWork unitOfWork,
        IChannelFactory channelFactory,
        ILogger<EmailNotificationService> logger) : IEmailNotificationService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IChannelFactory _channelFactory = channelFactory;
        private readonly ILogger<EmailNotificationService> _logger = logger;

        private const NotificationChannel EmailChannel = NotificationChannel.EMAIL;
        private const NotificationChannel InAppChannel = NotificationChannel.InApp;

        public async Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default)
        {
            var expiryMinutes = CalculateExpiryMinutes(message.ExpiryTime);
            var title = NotificationConstants.Titles.Otp;
            var messageContent = $"Mã xác thực OTP Verendar của bạn là: {message.Otp}. Hiệu lực {Math.Ceiling(expiryMinutes)} phút. Không chia sẻ mã này.";

            var notification = message.OtpRequestedToNotificationEntity(title, messageContent, NotificationType.System, isFallback: false);
            var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var context = message.ToDeliveryContext(notification.Id, title, messageContent, notification.NotificationType, expiryMinutes);
            return await SendEmailDeliveryAsync(notification, delivery, context, cancellationToken);
        }

        public async Task<bool> SendWelcomeEmailAsync(UserRegisteredEvent message, CancellationToken cancellationToken = default)
        {
            var title = NotificationConstants.Titles.Welcome;
            var messageContent = $"Chào mừng {message.FullName} đến với Verendar!";

            var notification = message.UserRegisteredToNotificationEntity(title, messageContent, NotificationType.Welcome);
            await _unitOfWork.Notifications.AddAsync(notification);

            NotificationDelivery? emailDelivery = null;
            if (!string.IsNullOrWhiteSpace(message.Email))
            {
                emailDelivery = notification.CreateDelivery(message.Email, EmailChannel);
                await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
            }
            await _unitOfWork.NotificationDeliveries.AddAsync(notification.CreateDelivery(message.UserId.ToString(), InAppChannel));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (emailDelivery == null)
            {
                _logger.LogDebug("UserRegisteredEvent has no Email for UserId {UserId}, only InApp delivery created", message.UserId);
                return false;
            }

            var templateModel = message.ToWelcomeEmailModel();
            var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                notification.Id, message.Email!, title, messageContent,
                notification.NotificationType, templateModel, NotificationConstants.TemplateKeys.Welcome);

            return await SendEmailDeliveryAsync(notification, emailDelivery, deliveryContext, cancellationToken);
        }

        public async Task<(bool EmailSent, Guid? NotificationId)> SendOdometerReminderAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default)
        {
            var days = message.StaleOdometerDays > 0 ? message.StaleOdometerDays : 3;
            var messageContent = $"Bạn đã không cập nhật số km (odo) trong {days} ngày qua. "
                + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";
            var title = OdometerReminderMappings.OdometerReminderTitle;

            var notification = message.OdometerReminderToNotificationEntity(title, messageContent);
            await _unitOfWork.Notifications.AddAsync(notification);

            NotificationDelivery? emailDelivery = null;
            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                emailDelivery = notification.CreateDelivery(message.TargetValue, EmailChannel);
                await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
            }

            var inAppDelivery = notification.CreateDelivery(message.UserId.ToString(), InAppChannel);
            await _unitOfWork.NotificationDeliveries.AddAsync(inAppDelivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var emailSent = false;
            if (emailDelivery != null)
            {
                var templateModel = message.ToEmailModel(days);
                var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                    notification.Id, message.TargetValue!, title, messageContent,
                    notification.NotificationType, templateModel, NotificationConstants.TemplateKeys.OdometerReminder);

                emailSent = await SendEmailDeliveryAsync(notification, emailDelivery, deliveryContext, cancellationToken);
            }
            else
            {
                _logger.LogDebug("OdometerReminderEvent has no TargetValue (email) for UserId {UserId}, only InApp delivery created", message.UserId);
            }

            return (emailSent, notification.Id);
        }

        public async Task<(bool EmailSent, IReadOnlyList<Guid> NotificationIds)> SendMaintenanceReminderAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default)
        {
            var items = message.Items ?? [];
            var isCriticalWithParts = message.Level >= ReminderLevel.Critical && items.Count > 0;

            if (isCriticalWithParts)
            {
                var notificationIds = new List<Guid>();
                var allEmailSent = true;

                foreach (var item in items)
                {
                    var singleItemMessage = message.ToSingleItemMessage(item);
                    var (title, messageContent) = item.BuildSingleItemCriticalContent();
                    var notification = singleItemMessage.MaintenanceReminderToNotificationEntity(title, messageContent);
                    await _unitOfWork.Notifications.AddAsync(notification);

                    NotificationDelivery? emailDelivery = null;
                    if (!string.IsNullOrWhiteSpace(message.TargetValue))
                    {
                        emailDelivery = notification.CreateDelivery(message.TargetValue, EmailChannel);
                        await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
                    }
                    await _unitOfWork.NotificationDeliveries.AddAsync(notification.CreateDelivery(message.UserId.ToString(), InAppChannel));
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (emailDelivery != null)
                    {
                        var templateModel = message.ToEmailModel(title, messageContent, item);
                        var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                            notification.Id, message.TargetValue!, title, messageContent,
                            notification.NotificationType, templateModel, NotificationConstants.TemplateKeys.MaintenanceReminder);

                        var sent = await SendEmailDeliveryAsync(notification, emailDelivery, deliveryContext, cancellationToken,
                            $"Part: {item.PartCategoryName}");
                        if (!sent) allEmailSent = false;
                    }

                    notificationIds.Add(notification.Id);
                }

                return (allEmailSent, notificationIds);
            }

            // Non-critical: single notification for all items
            var (singleTitle, singleMessageContent) = message.BuildContent();
            var singleNotification = message.MaintenanceReminderToNotificationEntity(singleTitle, singleMessageContent);
            await _unitOfWork.Notifications.AddAsync(singleNotification);

            NotificationDelivery? singleEmailDelivery = null;
            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                singleEmailDelivery = singleNotification.CreateDelivery(message.TargetValue, EmailChannel);
                await _unitOfWork.NotificationDeliveries.AddAsync(singleEmailDelivery);
            }
            await _unitOfWork.NotificationDeliveries.AddAsync(singleNotification.CreateDelivery(message.UserId.ToString(), InAppChannel));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var singleEmailSent = false;
            if (singleEmailDelivery != null)
            {
                var templateModel = message.ToEmailModel(singleTitle, singleMessageContent, items);
                var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                    singleNotification.Id, message.TargetValue!, singleTitle, singleMessageContent,
                    singleNotification.NotificationType, templateModel, NotificationConstants.TemplateKeys.MaintenanceReminder);

                singleEmailSent = await SendEmailDeliveryAsync(singleNotification, singleEmailDelivery, deliveryContext, cancellationToken);
            }
            else
            {
                _logger.LogDebug("MaintenanceReminderEvent has no TargetValue (email) for UserId {UserId}, only InApp delivery created", message.UserId);
            }

            return (singleEmailSent, [singleNotification.Id]);
        }

        // Unified email send + delivery status update — used by all three send methods.
        private async Task<bool> SendEmailDeliveryAsync(
            NotificationEntity notification,
            NotificationDelivery delivery,
            NotificationDeliveryContext deliveryContext,
            CancellationToken cancellationToken,
            string? extraContext = null)
        {
            try
            {
                var channel = _channelFactory.GetChannel(EmailChannel);
                var result = await channel.SendAsync(deliveryContext);

                if (result.IsSuccess)
                {
                    notification.Status = NotificationStatus.Sent;
                    delivery.Status = NotificationStatus.Sent;
                    delivery.SentAt = delivery.DeliveredAt = DateTime.UtcNow;
                }
                else
                {
                    delivery.Status = NotificationStatus.Failed;
                    delivery.ErrorMessage = result.ErrorMessage;
                    delivery.RetryCount++;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                var context = extraContext is null
                    ? $"NotificationId: {notification.Id}"
                    : $"NotificationId: {notification.Id}, {extraContext}";
                _logger.LogError(ex, "Error sending email delivery - {Context}", context);

                delivery.Status = NotificationStatus.Failed;
                delivery.ErrorMessage = ex.Message;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return false;
            }
        }

        private static double CalculateExpiryMinutes(DateTime expiryTime)
        {
            var timeSpan = expiryTime - DateTime.UtcNow;
            return timeSpan.TotalMinutes > 0 ? timeSpan.TotalMinutes : 0;
        }
    }
}
