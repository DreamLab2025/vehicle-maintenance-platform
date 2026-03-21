using System.ComponentModel.DataAnnotations.Schema;

namespace Verendar.Vehicle.Domain.Entities
{
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReferencePrice { get; set; }

        public int? RecommendedKmInterval { get; set; }

        public int? RecommendedMonthsInterval { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public PartCategory Category { get; set; } = null!;

        public List<PartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
