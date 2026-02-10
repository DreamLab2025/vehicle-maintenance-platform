using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Email;
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
        private const string OtpEmailTitle = "Mã xác thực OTP";

        public async Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default)
        {
            var expiryMinutes = Math.Ceiling(CalculateExpiryMinutes(message.ExpiryTime));
            var messageContent = $"Mã xác thực OTP Verendar của bạn là: {message.Otp}. Hiệu lực {expiryMinutes} phút. Không chia sẻ mã này.";

            var notification = message.OtpRequestedToNotificationEntity(
                OtpEmailTitle,
                messageContent,
                NotificationType.System,
                isFallback: false);
            var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var context = new NotificationDeliveryContext
            {
                NotificationId = notification.Id,
                RecipientEmail = message.TargetValue,
                RecipientPhone = null,
                Title = OtpEmailTitle,
                Message = messageContent,
                NotificationType = notification.NotificationType,
                TemplateParameters = new Dictionary<string, string>
                {
                    { "OTP", message.Otp },
                    { "OtpCode", message.Otp },
                    { "ExpiryMinutes", CalculateExpiryMinutes(message.ExpiryTime).ToString() },
                    { "ExpiryTime", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy") },
                    { "Type", message.Type.ToString() }
                },
                Metadata = new Dictionary<string, object> { { "TemplateKey", "Otp" } }
            };

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

        private const string OdometerReminderTitle = "Nhắc nhở cập nhật số km";

        public async Task<(bool EmailSent, Guid? NotificationId)> SendOdometerReminderAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default)
        {
            var days = message.StaleOdometerDays > 0 ? message.StaleOdometerDays : 3;
            var messageContent = $"Bạn đã không cập nhật số km (odo) trong {days} ngày qua. "
                + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";

            var notification = message.OdometerReminderToNotificationEntity(
                OdometerReminderTitle,
                messageContent);

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
                var templateModel = new OdometerReminderEmailModel
                {
                    UserName = message.UserName,
                    UserEmail = message.TargetValue,
                    Title = OdometerReminderTitle,
                    StaleOdometerDays = days,
                    Vehicles = (message.Vehicles ?? []).Select(v => new OdometerReminderVehicleEmailDto
                    {
                        VehicleDisplayName = v.VehicleDisplayName,
                        LicensePlate = v.LicensePlate,
                        CurrentOdometer = v.CurrentOdometer,
                        LastOdometerUpdateFormatted = v.LastOdometerUpdate?.ToString("dd/MM/yyyy"),
                        DaysSinceUpdate = v.DaysSinceUpdate
                    }).ToList()
                };

                var deliveryContext = new NotificationDeliveryContext
                {
                    NotificationId = notification.Id,
                    RecipientEmail = message.TargetValue,
                    RecipientPhone = null,
                    Title = OdometerReminderTitle,
                    Message = messageContent,
                    NotificationType = notification.NotificationType,
                    Metadata = new Dictionary<string, object> { { "TemplateKey", "OdometerReminder" } },
                    TemplateModel = templateModel
                };

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

        public async Task<(bool EmailSent, Guid? NotificationId)> SendMaintenanceReminderAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default)
        {
            var (title, messageContent) = BuildMaintenanceReminderContent(message);

            var notification = message.MaintenanceReminderToNotificationEntity(title, messageContent);

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
                const int CriticalLevel = 4;
                var templateModel = new MaintenanceReminderEmailModel
                {
                    UserName = message.UserName,
                    UserEmail = message.TargetValue,
                    Title = title,
                    LevelName = message.LevelName,
                    IsCritical = message.Level >= CriticalLevel,
                    Items = (message.Items ?? []).Select(i => new MaintenanceReminderItemEmailDto
                    {
                        PartCategoryName = i.PartCategoryName,
                        Description = i.Description,
                        VehicleDisplayName = i.VehicleDisplayName,
                        CurrentOdometer = i.CurrentOdometer,
                        TargetOdometer = i.TargetOdometer,
                        PercentageRemaining = i.PercentageRemaining,
                        EstimatedNextReplacementDate = i.EstimatedNextReplacementDate?.ToString("dd/MM/yyyy")
                    }).ToList()
                };

                var deliveryContext = new NotificationDeliveryContext
                {
                    NotificationId = notification.Id,
                    RecipientEmail = message.TargetValue,
                    RecipientPhone = null,
                    Title = title,
                    Message = messageContent,
                    NotificationType = notification.NotificationType,
                    Metadata = new Dictionary<string, object> { { "TemplateKey", "MaintenanceReminder" } },
                    TemplateModel = templateModel
                };

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
                    _logger.LogError(ex, "Error sending MaintenanceReminder email - NotificationId: {NotificationId}", notification.Id);
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
                _logger.LogDebug("MaintenanceReminderEvent has no TargetValue (email) for UserId {UserId}, only InApp delivery created", message.UserId);
            }

            return (emailSent, notification.Id);
        }

        private static (string Title, string Message) BuildMaintenanceReminderContent(MaintenanceReminderEvent message)
        {
            const int CriticalLevel = 4;
            var partList = message.Items.Count > 0
                ? string.Join("\n", message.Items.Select(i => $"• {i.PartCategoryName} (số km hiện tại: {i.CurrentOdometer:N0}, cần thay trước: {i.TargetOdometer:N0})"))
                : "Các linh kiện cần bảo dưỡng/thay thế.";

            if (message.Level >= CriticalLevel)
            {
                var title = "Khẩn cấp: Cần thay linh kiện";
                var body = "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. "
                    + "Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện (về mức bình thường).\n\n"
                    + "Các linh kiện cần chú ý:\n" + partList
                    + "\n\nVui lòng vào app cập nhật sau khi thay linh kiện để dừng nhắc nhở.";
                return (title, body);
            }

            var normalTitle = $"Nhắc nhở bảo dưỡng ({message.LevelName})";
            var normalBody = "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:\n\n" + partList;
            return (normalTitle, normalBody);
        }
    }
}
