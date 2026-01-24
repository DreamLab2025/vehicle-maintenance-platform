using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Theo dõi từng phụ tùng của từng xe người dùng
    /// Thay thế UserMaintenanceConfig với thiết kế mới
    /// </summary>
    public class VehiclePartTracking : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        /// <summary>
        /// Sản phẩm hiện đang sử dụng (null nếu chưa biết)
        /// Khi user thay phụ tùng, update field này
        /// </summary>
        public Guid? CurrentPartProductId { get; set; }

        /// <summary>
        /// Instance identifier cho các phụ tùng có nhiều instances
        /// Ví dụ: "FRONT_LEFT", "FRONT_RIGHT", "REAR_LEFT", "REAR_RIGHT" cho lốp
        /// null cho phụ tùng chỉ có 1 instance (nhớt, ắc quy...)
        /// </summary>
        [MaxLength(50)]
        public string? InstanceIdentifier { get; set; }

        // Last replacement info
        /// <summary>
        /// Lần thay gần nhất ở km nào
        /// </summary>
        public int? LastReplacementOdometer { get; set; }

        /// <summary>
        /// Ngày thay gần nhất
        /// </summary>
        public DateOnly? LastReplacementDate { get; set; }

        // Custom schedule (override default)
        /// <summary>
        /// Chu kỳ custom theo km (null = dùng của PartProduct hoặc DefaultSchedule)
        /// </summary>
        public int? CustomKmInterval { get; set; }

        /// <summary>
        /// Chu kỳ custom theo tháng (null = dùng của PartProduct hoặc DefaultSchedule)
        /// </summary>
        public int? CustomMonthsInterval { get; set; }

        // Prediction (tự động tính)
        /// <summary>
        /// Dự đoán km tiếp theo cần thay
        /// = LastReplacementOdometer + Interval
        /// </summary>
        public int? PredictedNextOdometer { get; set; }

        /// <summary>
        /// Dự đoán ngày tiếp theo cần thay
        /// Tính từ LastReplacementDate + MonthsInterval
        /// Hoặc dựa vào AverageKmPerDay
        /// </summary>
        public DateOnly? PredictedNextDate { get; set; }

        /// <summary>
        /// User không muốn track phụ tùng này
        /// </summary>
        public bool IsIgnored { get; set; } = false;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public UserVehicle UserVehicle { get; set; } = null!;

        public PartCategory PartCategory { get; set; } = null!;

        public PartProduct? CurrentPartProduct { get; set; }

        public List<MaintenanceReminder> Reminders { get; set; } = [];
    }
}
