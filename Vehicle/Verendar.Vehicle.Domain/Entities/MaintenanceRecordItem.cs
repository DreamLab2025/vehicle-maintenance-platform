using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceRecordItem : BaseEntity
    {
        [Required]
        public Guid MaintenanceRecordId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        public Guid? PartProductId { get; set; }

        [MaxLength(200)]
        public string? CustomPartName { get; set; }

        [MaxLength(50)]
        public string? InstanceIdentifier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool UpdatesTracking { get; set; } = true;

        public MaintenanceRecord MaintenanceRecord { get; set; } = null!;

        public PartCategory PartCategory { get; set; } = null!;

        public PartProduct? PartProduct { get; set; }
    }
}
