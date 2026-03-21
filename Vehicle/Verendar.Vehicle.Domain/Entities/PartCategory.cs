namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(DisplayOrder))]
    public class PartCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? IconUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool RequiresOdometerTracking { get; set; } = true;

        public bool RequiresTimeTracking { get; set; } = true;

        public bool AllowsMultipleInstances { get; set; } = false;

        [MaxLength(1000)]
        public string? IdentificationSigns { get; set; }

        [MaxLength(1000)]
        public string? ConsequencesIfNotHandled { get; set; }

        public List<PartProduct> Products { get; set; } = [];
        public List<DefaultMaintenanceSchedule> DefaultSchedules { get; set; } = [];
        public List<PartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
