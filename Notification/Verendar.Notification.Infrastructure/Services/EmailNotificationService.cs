using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Events;

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

            var notification = message.OtpRequestedToNotificationEntity(
                title,
                messageContent,
                NotificationType.System,
                isFallback: false);
            var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var context = message.ToDeliveryContext(notification.Id, title, messageContent, notification.NotificationType, expiryMinutes);

            try
            {
                var channelService = _channelFactory.GetChannel(EmailChannel);
                var result = await channelService.SendAsync(context);

                if (result.IsSuccess)
                {
                    notification.Status = NotificationStatus.Sent;
                    delivery.Status = NotificationStatus.Sent;
                    delivery.SentAt = DateTime.UtcNow;
                    delivery.DeliveredAt = DateTime.UtcNow;
                }
                else
                {
                    notification.Status = NotificationStatus.Failed;
                    delivery.Status = NotificationStatus.Failed;
                    delivery.ErrorMessage = result.ErrorMessage;
                    delivery.RetryCount++;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email - NotificationId: {NotificationId}", notification.Id);
                notification.Status = NotificationStatus.Failed;
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

        public async Task<(bool EmailSent, Guid? NotificationId)> SendOdometerReminderAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default)
        {
            var days = message.StaleOdometerDays > 0 ? message.StaleOdometerDays : 3;
            var messageContent = $"Bạn đã không cập nhật số km (odo) trong {days} ngày qua. "
                + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";

            var title = OdometerReminderMappings.OdometerReminderTitle;
            var notification = message.OdometerReminderToNotificationEntity(title, messageContent);

            await _unitOfWork.Notifications.AddAsync(notification);

            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                var emailDelivery = notification.CreateDelivery(message.TargetValue, EmailChannel);
                await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
            }

            var inAppDelivery = notification.CreateDelivery(message.UserId.ToString(), InAppChannel);
            await _unitOfWork.NotificationDeliveries.AddAsync(inAppDelivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var emailSent = false;
            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                var templateModel = message.ToEmailModel(days);
                var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                    notification.Id, message.TargetValue!, title, messageContent,
                    notification.NotificationType, templateModel, NotificationConstants.TemplateKeys.OdometerReminder);

                try
                {
                    var channelService = _channelFactory.GetChannel(EmailChannel);
                    var result = await channelService.SendAsync(deliveryContext);
                    var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                        d.NotificationId == notification.Id && d.Channel == EmailChannel);
                    if (emailDeliveryEntity != null)
                    {
                        if (result.IsSuccess)
                        {
                            notification.Status = NotificationStatus.Sent;
                            emailDeliveryEntity.Status = NotificationStatus.Sent;
                            emailDeliveryEntity.SentAt = DateTime.UtcNow;
                            emailDeliveryEntity.DeliveredAt = DateTime.UtcNow;
                            emailSent = true;
                        }
                        else
                        {
                            emailDeliveryEntity.Status = NotificationStatus.Failed;
                            emailDeliveryEntity.ErrorMessage = result.ErrorMessage;
                            emailDeliveryEntity.RetryCount++;
                        }
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending OdometerReminder email - NotificationId: {NotificationId}", notification.Id);
                    var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                        d.NotificationId == notification.Id && d.Channel == EmailChannel);
                    if (emailDeliveryEntity != null)
                    {
                        emailDeliveryEntity.Status = NotificationStatus.Failed;
                        emailDeliveryEntity.ErrorMessage = ex.Message;
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
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
            var isCriticalWithParts = message.Level >= MaintenanceReminderMappings.CriticalLevel && items.Count > 0;

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

                    if (!string.IsNullOrWhiteSpace(message.TargetValue))
                    {
                        var emailDelivery = notification.CreateDelivery(message.TargetValue, EmailChannel);
                        await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
                    }
                    var inAppDelivery = notification.CreateDelivery(message.UserId.ToString(), InAppChannel);
                    await _unitOfWork.NotificationDeliveries.AddAsync(inAppDelivery);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (!string.IsNullOrWhiteSpace(message.TargetValue))
                    {
                        var templateModel = message.ToEmailModel(title, messageContent, item);
                        var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                            notification.Id, message.TargetValue, title, messageContent,
                            notification.NotificationType, templateModel, NotificationConstants.TemplateKeys.MaintenanceReminder);
                        try
                        {
                            var channelService = _channelFactory.GetChannel(EmailChannel);
                            var result = await channelService.SendAsync(deliveryContext);
                            var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                                d.NotificationId == notification.Id && d.Channel == EmailChannel);
                            if (emailDeliveryEntity != null)
                            {
                                if (result.IsSuccess)
                                {
                                    notification.Status = NotificationStatus.Sent;
                                    emailDeliveryEntity.Status = NotificationStatus.Sent;
                                    emailDeliveryEntity.SentAt = DateTime.UtcNow;
                                    emailDeliveryEntity.DeliveredAt = DateTime.UtcNow;
                                }
                                else
                                {
                                    emailDeliveryEntity.Status = NotificationStatus.Failed;
                                    emailDeliveryEntity.ErrorMessage = result.ErrorMessage;
                                    emailDeliveryEntity.RetryCount++;
                                    allEmailSent = false;
                                }
                                await _unitOfWork.SaveChangesAsync(cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending MaintenanceReminder email - NotificationId: {NotificationId}, Part: {PartName}", notification.Id, item.PartCategoryName);
                            var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                                d.NotificationId == notification.Id && d.Channel == EmailChannel);
                            if (emailDeliveryEntity != null)
                            {
                                emailDeliveryEntity.Status = NotificationStatus.Failed;
                                emailDeliveryEntity.ErrorMessage = ex.Message;
                                await _unitOfWork.SaveChangesAsync(cancellationToken);
                            }
                            allEmailSent = false;
                        }
                    }
                    notificationIds.Add(notification.Id);
                }
                return (allEmailSent, notificationIds);
            }

            var (singleTitle, singleMessageContent) = message.BuildContent();
            var singleNotification = message.MaintenanceReminderToNotificationEntity(singleTitle, singleMessageContent);
            await _unitOfWork.Notifications.AddAsync(singleNotification);

            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                var emailDelivery = singleNotification.CreateDelivery(message.TargetValue, EmailChannel);
                await _unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
            }
            var singleInAppDelivery = singleNotification.CreateDelivery(message.UserId.ToString(), InAppChannel);
            await _unitOfWork.NotificationDeliveries.AddAsync(singleInAppDelivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var singleEmailSent = false;
            if (!string.IsNullOrWhiteSpace(message.TargetValue))
            {
                var templateModel = message.ToEmailModel(singleTitle, singleMessageContent, items);
                var deliveryContext = EmailDeliveryContextMappings.ToDeliveryContext(
                    singleNotification.Id, message.TargetValue, singleTitle, singleMessageContent,
                    singleNotification.NotificationType, templateModel, NotificationConstants.TemplateKeys.MaintenanceReminder);
                try
                {
                    var channelService = _channelFactory.GetChannel(EmailChannel);
                    var result = await channelService.SendAsync(deliveryContext);
                    var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                        d.NotificationId == singleNotification.Id && d.Channel == EmailChannel);
                    if (emailDeliveryEntity != null)
                    {
                        if (result.IsSuccess)
                        {
                            singleNotification.Status = NotificationStatus.Sent;
                            emailDeliveryEntity.Status = NotificationStatus.Sent;
                            emailDeliveryEntity.SentAt = DateTime.UtcNow;
                            emailDeliveryEntity.DeliveredAt = DateTime.UtcNow;
                            singleEmailSent = true;
                        }
                        else
                        {
                            emailDeliveryEntity.Status = NotificationStatus.Failed;
                            emailDeliveryEntity.ErrorMessage = result.ErrorMessage;
                            emailDeliveryEntity.RetryCount++;
                        }
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending MaintenanceReminder email - NotificationId: {NotificationId}", singleNotification.Id);
                    var emailDeliveryEntity = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                        d.NotificationId == singleNotification.Id && d.Channel == EmailChannel);
                    if (emailDeliveryEntity != null)
                    {
                        emailDeliveryEntity.Status = NotificationStatus.Failed;
                        emailDeliveryEntity.ErrorMessage = ex.Message;
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            else
            {
                _logger.LogDebug("MaintenanceReminderEvent has no TargetValue (email) for UserId {UserId}, only InApp delivery created", message.UserId);
            }

            return (singleEmailSent, [singleNotification.Id]);
        }

    }
}
