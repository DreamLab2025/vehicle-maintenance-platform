using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Chi tiết từng phụ tùng thay trong hồ sơ bảo dưỡng
    /// Thay thế MaintenanceActivityDetail với thiết kế mới
    /// </summary>
    public class MaintenanceRecordItem : BaseEntity
    {
        [Required]
        public Guid MaintenanceRecordId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        /// <summary>
        /// Sản phẩm cụ thể đã thay (nullable nếu user không rõ hoặc không chọn)
        /// </summary>
        public Guid? PartProductId { get; set; }

        /// <summary>
        /// Instance identifier (cho lốp: "FRONT_LEFT", cho nhớt: null)
        /// </summary>
        [MaxLength(50)]
        public string? InstanceIdentifier { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Có cập nhật VehiclePartTracking không
        /// true = sau khi save, tự động update LastReplacementOdometer/Date
        /// false = chỉ lưu lịch sử, không update tracking (ví dụ: rửa xe)
        /// </summary>
        public bool UpdatesTracking { get; set; } = true;

        // Navigation properties
        public MaintenanceRecord MaintenanceRecord { get; set; } = null!;

        public PartCategory PartCategory { get; set; } = null!;

        public PartProduct? PartProduct { get; set; }
    }
}
