namespace Verendar.Notification.Domain.Entities
{
    [Index(nameof(Code), IsUnique = true)]
    public class NotificationTemplate : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TitleTemplate { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string MessageTemplate { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }

        public NotificationPriority DefaultPriority { get; set; } = NotificationPriority.Medium;

        [Column(TypeName = "jsonb")]
        public string? VariablesJson { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public ICollection<NotificationTemplateChannel> Channels { get; set; } = new List<NotificationTemplateChannel>();
    }

}
