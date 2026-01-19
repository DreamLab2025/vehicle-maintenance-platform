using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    /// Phụ tùng xe (đã đổi tên từ ConsumableItem)
    public class VehiclePart : BaseEntity
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; } // Đơn vị: lít, cái, bộ, v.v.

        /// <summary>
        /// Mã SKU hoặc mã sản phẩm (nếu có)
        /// </summary>
        [MaxLength(100)]
        public string? Sku { get; set; }

        /// <summary>
        /// Giá tham khảo (có thể để null nếu không có)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReferencePrice { get; set; }

        /// <summary>
        /// Phân loại phụ tùng (Dầu nhớt, Lốp, Ắc quy, v.v.)
        /// </summary>
        [Required]
        public Guid CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public VehiclePartCategory Category { get; set; } = null!;

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public ICollection<StandardMaintenanceSchedule> StandardMaintenanceSchedules { get; set; } = new List<StandardMaintenanceSchedule>();
        public ICollection<MaintenanceActivityDetail> MaintenanceActivityDetails { get; set; } = new List<MaintenanceActivityDetail>();
        public ICollection<UserMaintenanceConfig> UserMaintenanceConfigs { get; set; } = new List<UserMaintenanceConfig>();
    }
}
