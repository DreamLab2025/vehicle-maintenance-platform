namespace Verendar.Notification.Domain.Entities
{
    [Index(nameof(NotificationId), nameof(Channel))]
    public class NotificationDelivery : BaseEntity
    {
        [Required]
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;

        [Required]
        public NotificationChannel Channel { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [MaxLength(200)]
        public string? RecipientAddress { get; set; }

        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [MaxLength(200)]
        public string? ExternalId { get; set; }
    }
}