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

        [MaxLength(20)]
        public string? LicensePlate { get; set; }

        [MaxLength(17)]
        public string? VIN { get; set; }

        public DateOnly? PurchaseDate { get; set; }

        public int CurrentOdometer { get; set; }

        public DateOnly? LastOdometerUpdate { get; set; }

        public int? AverageKmPerDay { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public VehicleVariant Variant { get; set; } = null!;

        public ICollection<OdometerHistory> OdometerHistory { get; set; } = new List<OdometerHistory>();
        public ICollection<VehiclePartTracking> PartTrackings { get; set; } = new List<VehiclePartTracking>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    }
}
