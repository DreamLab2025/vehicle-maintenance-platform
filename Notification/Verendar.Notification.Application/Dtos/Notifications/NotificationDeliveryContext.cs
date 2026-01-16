using System;

namespace Verendar.Notification.Application.Dtos.Notifications;

public class NotificationDeliveryContext
{
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public string? ZaloTemplateId { get; set; }
    public Dictionary<string, string>? TemplateParameters { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
