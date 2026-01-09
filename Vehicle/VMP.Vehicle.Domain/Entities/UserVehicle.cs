using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class UserVehicle : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid VehicleVariantId { get; set; }

        [ForeignKey(nameof(VehicleVariantId))]
        public VehicleVariant VehicleVariant { get; set; } = null!;

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = null!;

        [MaxLength(100)]
        public string? Nickname { get; set; } // "Xe đi phượt"

        [MaxLength(50)]
        public string? VinNumber { get; set; }

        public DateTime PurchaseDate { get; set; }

        public int CurrentOdometer { get; set; }
        public DateTime LastOdometerUpdateAt { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AverageKmPerDay { get; set; }

        public DateTime? LastCalculatedDate { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public ICollection<OdometerHistory> OdometerHistories { get; set; } = new List<OdometerHistory>();
        public ICollection<UserMaintenanceConfig> MaintenanceConfigs { get; set; } = new List<UserMaintenanceConfig>();
    }
}
