namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceQuestionGroup : BaseEntity
    {
        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public List<MaintenanceQuestion> Questions { get; set; } = [];
    }
}
