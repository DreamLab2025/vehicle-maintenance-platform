namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(PartCategoryId))]
    public class PartProduct : BaseEntity
    {
        [Required]
        public Guid PartCategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public decimal? ReferencePrice { get; set; }

        public int? RecommendedKmInterval { get; set; }

        public int? RecommendedMonthsInterval { get; set; }

        public PartCategory Category { get; set; } = null!;

        public List<PartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
