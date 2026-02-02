using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
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

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public bool RequiresOdometerTracking { get; set; } = true;

        public bool RequiresTimeTracking { get; set; } = true;

        public bool AllowsMultipleInstances { get; set; } = false;

        /// <summary>
        /// Dấu hiệu nhận biết cần bảo trì/thay thế (ví dụ: dầu đen, tiếng kêu, rung).
        /// </summary>
        [MaxLength(1000)]
        public string? IdentificationSigns { get; set; }

        /// <summary>
        /// Hậu quả nếu không xử lý kịp thời (ví dụ: hỏng động cơ, mất phanh).
        /// </summary>
        [MaxLength(1000)]
        public string? ConsequencesIfNotHandled { get; set; }

        public List<PartProduct> Products { get; set; } = [];
        public List<DefaultMaintenanceSchedule> DefaultSchedules { get; set; } = [];
        public List<VehiclePartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
