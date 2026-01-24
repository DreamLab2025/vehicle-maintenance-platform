using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class UserVehicle : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid VehicleVariantId { get; set; }

        // Registration info
        [MaxLength(20)]
        public string? LicensePlate { get; set; }

        [MaxLength(17)]
        public string? VIN { get; set; }

        public DateOnly? PurchaseDate { get; set; }

        // Current status
        public int CurrentOdometer { get; set; }

        public DateOnly? LastOdometerUpdate { get; set; }

        /// <summary>
        /// Trung bình km/ngày, dùng để predict ngày cần thay phụ tùng
        /// Tự động tính từ OdometerHistory
        /// </summary>
        public int? AverageKmPerDay { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public VehicleVariant Variant { get; set; } = null!;

        public ICollection<OdometerHistory> OdometerHistory { get; set; } = new List<OdometerHistory>();
        public ICollection<VehiclePartTracking> PartTrackings { get; set; } = new List<VehiclePartTracking>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    }
}
