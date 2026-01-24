using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Lịch bảo dưỡng mặc định theo khuyến nghị của hãng xe cho từng model
    /// Ví dụ: Honda Wave Alpha 110 khuyến nghị thay nhớt mỗi 2000km hoặc 6 tháng
    /// </summary>
    public class DefaultMaintenanceSchedule : BaseEntity
    {
        [Required]
        public Guid VehicleModelId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        /// <summary>
        /// Km ban đầu cần thay (lần đầu tiên)
        /// Ví dụ: Nhớt lần đầu ở 1000km
        /// </summary>
        public int InitialKm { get; set; }

        /// <summary>
        /// Khoảng cách km giữa các lần thay
        /// Ví dụ: 2000km
        /// </summary>
        public int KmInterval { get; set; }

        /// <summary>
        /// Khoảng thời gian (tháng) giữa các lần thay
        /// Ví dụ: 6 tháng
        /// </summary>
        public int MonthsInterval { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public VehicleModel VehicleModel { get; set; } = null!;
        public PartCategory PartCategory { get; set; } = null!;
    }
}
