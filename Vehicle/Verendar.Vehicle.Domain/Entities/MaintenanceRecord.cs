using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Hồ sơ bảo dưỡng - ghi lại mỗi lần đi bảo dưỡng xe
    /// Thay thế MaintenanceActivity với thiết kế mới
    /// </summary>
    public class MaintenanceRecord : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        /// <summary>
        /// Ngày bảo dưỡng
        /// </summary>
        public DateOnly ServiceDate { get; set; }

        /// <summary>
        /// Số km khi bảo dưỡng
        /// </summary>
        public int OdometerAtService { get; set; }

        // Garage information
        [MaxLength(200)]
        public string? GarageName { get; set; }

        // Financial
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? InvoiceImageUrl { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public UserVehicle UserVehicle { get; set; } = null!;

        public List<MaintenanceRecordItem> Items { get; set; } = [];
    }
}
