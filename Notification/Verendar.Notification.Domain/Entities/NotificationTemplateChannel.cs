using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Domain.Entities
{
    public class NotificationTemplateChannel : BaseEntity
    {
        [Required]
        public Guid NotificationTemplateId { get; set; }
        public NotificationTemplate NotificationTemplate { get; set; } = null!;

        [Required]
        public NotificationChannel Channel { get; set; }

        public bool IsEnabled { get; set; } = true;

        [MaxLength(100)]
        public string? ExternalTemplateId { get; set; }
    }
}
