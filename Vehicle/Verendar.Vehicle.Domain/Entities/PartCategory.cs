using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Danh mục phụ tùng (Dầu nhớt, Lốp xe, Ắc quy...)
    /// </summary>
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

        // Navigation properties
        public List<PartProduct> Products { get; set; } = [];
        public List<DefaultMaintenanceSchedule> DefaultSchedules { get; set; } = [];
        public List<VehiclePartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
