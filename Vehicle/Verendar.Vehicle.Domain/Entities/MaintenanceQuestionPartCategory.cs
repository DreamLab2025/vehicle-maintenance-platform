namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceQuestionPartCategory : BaseEntity
    {
        public Guid MaintenanceQuestionId { get; set; }

        public MaintenanceQuestion MaintenanceQuestion { get; set; } = null!;

        public Guid PartCategoryId { get; set; }

        public PartCategory PartCategory { get; set; } = null!;
    }
}
