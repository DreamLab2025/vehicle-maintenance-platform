using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Nhắc nhở bảo dưỡng cho từng phụ tùng
    /// Được tạo/cập nhật khi user cập nhật odometer
    /// </summary>
    public class MaintenanceReminder : BaseEntity
    {
        [Required]
        public Guid VehiclePartTrackingId { get; set; }

        /// <summary>
        /// Odometer hiện tại khi tính reminder
        /// </summary>
        public int CurrentOdometer { get; set; }

        /// <summary>
        /// Odometer mục tiêu cần thay
        /// </summary>
        public int TargetOdometer { get; set; }

        /// <summary>
        /// Ngày mục tiêu cần thay (dựa vào time hoặc predict từ km)
        /// </summary>
        public DateOnly? TargetDate { get; set; }

        /// <summary>
        /// Mức độ nhắc nhở dựa trên % còn lại
        /// Low (>50%), Medium (25-50%), High (10-25%), Urgent (<10%)
        /// </summary>
        public ReminderLevel Level { get; set; }

        /// <summary>
        /// % còn lại đến hạn
        /// = (Target - Current) / (Target - Last) * 100
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentageRemaining { get; set; }

        // Notification status
        /// <summary>
        /// Đã gửi notification chưa
        /// </summary>
        public bool IsNotified { get; set; } = false;

        public DateOnly? NotifiedDate { get; set; }

        /// <summary>
        /// User đã dismiss reminder này
        /// </summary>
        public bool IsDismissed { get; set; } = false;

        public DateOnly? DismissedDate { get; set; }

        // Navigation properties
        public VehiclePartTracking PartTracking { get; set; } = null!;
    }
}
