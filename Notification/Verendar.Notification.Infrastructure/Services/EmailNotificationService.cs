using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Infrastructure.Services;

public class EmailNotificationService(
    IUnitOfWork unitOfWork,
    IChannelFactory channelFactory,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IChannelFactory _channelFactory = channelFactory;
    private readonly ILogger<EmailNotificationService> _logger = logger;

    private const NotificationChannel EmailChannel = NotificationChannel.EMAIL;
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

    public async Task<bool> SendOdometerReminderEmailAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.TargetValue))
        {
            _logger.LogWarning("OdometerReminderEvent has no TargetValue (email) for UserId {UserId}", message.UserId);
            return false;
        }

        var messageContent = "Bạn đã không cập nhật số km (odo) trong 3 ngày qua. "
            + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";

        var notification = message.OdometerReminderToNotificationEntity(
            OdometerReminderTitle,
            messageContent);
        var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var context = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientEmail = message.TargetValue,
            RecipientPhone = null,
            Title = OdometerReminderTitle,
            Message = messageContent,
            NotificationType = notification.NotificationType,
            Metadata = new Dictionary<string, object> { { "UserName", "bạn" } }
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
            _logger.LogError(ex, "Error sending OdometerReminder email - NotificationId: {NotificationId}", notification.Id);
            notification.Status = NotificationStatus.Failed;
            delivery.Status = NotificationStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    public async Task<bool> SendMaintenanceReminderEmailAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.TargetValue))
        {
            _logger.LogWarning("MaintenanceReminderEvent has no TargetValue (email) for UserId {UserId}", message.UserId);
            return false;
        }

        var (title, messageContent) = BuildMaintenanceReminderContent(message);

        var notification = message.MaintenanceReminderToNotificationEntity(title, messageContent);
        var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var context = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientEmail = message.TargetValue,
            RecipientPhone = null,
            Title = title,
            Message = messageContent,
            NotificationType = notification.NotificationType,
            Metadata = new Dictionary<string, object> { { "UserName", "bạn" }, { "Level", message.Level }, { "LevelName", message.LevelName } }
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
            _logger.LogError(ex, "Error sending MaintenanceReminder email - NotificationId: {NotificationId}", notification.Id);
            notification.Status = NotificationStatus.Failed;
            delivery.Status = NotificationStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private static (string Title, string Message) BuildMaintenanceReminderContent(MaintenanceReminderEvent message)
    {
        const int UrgentLevel = 4;
        var partList = message.Items.Count > 0
            ? string.Join("\n", message.Items.Select(i => $"• {i.PartCategoryName} (số km hiện tại: {i.CurrentOdometer:N0}, cần thay trước: {i.TargetOdometer:N0})"))
            : "Các linh kiện cần bảo dưỡng/thay thế.";

        if (message.Level >= UrgentLevel)
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
