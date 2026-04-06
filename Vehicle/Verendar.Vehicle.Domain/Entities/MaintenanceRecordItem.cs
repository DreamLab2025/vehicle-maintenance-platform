namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceRecordItem : BaseEntity
    {
        [Required]
        public Guid MaintenanceRecordId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        public Guid? GarageProductId { get; set; }

        [MaxLength(200)]
        public string? CustomPartName { get; set; }

        [MaxLength(50)]
        public string? InstanceIdentifier { get; set; }

        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool UpdatesTracking { get; set; } = true;

        public MaintenanceRecord MaintenanceRecord { get; set; } = null!;

        public PartCategory PartCategory { get; set; } = null!;
    }
}
