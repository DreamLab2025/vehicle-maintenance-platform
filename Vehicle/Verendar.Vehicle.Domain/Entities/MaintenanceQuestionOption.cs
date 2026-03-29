namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceQuestionOption : BaseEntity
    {
        public Guid QuestionId { get; set; }

        public MaintenanceQuestion Question { get; set; } = null!;

        [Required]
        [MaxLength(32)]
        public string OptionKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string ValueForAi { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
