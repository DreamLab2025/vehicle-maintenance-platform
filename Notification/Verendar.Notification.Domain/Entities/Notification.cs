namespace Verendar.Notification.Domain.Entities
{
    [Index(nameof(UserId), nameof(IsRead))]
    [Index(nameof(UserId), nameof(CreatedAt))]
    [Index(nameof(Status))]
    public class Notification : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [MaxLength(50)]
        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
    }
}
