namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceQuestion : BaseEntity
    {
        public Guid GroupId { get; set; }

        public MaintenanceQuestionGroup Group { get; set; } = null!;

        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string AiQuestion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Hint { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsAskOncePerSession { get; set; }

        public bool AppliesToAllPartCategories { get; set; }

        public bool Required { get; set; } = true;

        public List<MaintenanceQuestionOption> Options { get; set; } = [];

        public List<MaintenanceQuestionPartCategory> PartCategoryLinks { get; set; } = [];
    }
}
