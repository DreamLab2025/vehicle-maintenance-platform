using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class VehicleModel : BaseEntity
    {
        [Required]
        public Guid BrandId { get; set; }
        [ForeignKey(nameof(BrandId))]
        public VehicleBrand Brand { get; set; } = null!;

        [Required]
        public Guid TypeId { get; set; }
        [ForeignKey(nameof(TypeId))]
        public VehicleType Type { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        public int ReleaseYear { get; set; }
        public VehicleFuelType FuelType { get; set; }
        public VehicleTransmissionType TransmissionType { get; set; }

        public int? EngineDisplacement { get; set; } // Phân khối (cc)

        [Column(TypeName = "decimal(4,2)")]
        public decimal? EngineCapacity { get; set; } // Dung tích động cơ (L)

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OilCapacity { get; set; } // Dung tích dầu (liters)

        [MaxLength(50)]
        public string? TireSizeFront { get; set; } // Kích thước lốp trước
        [MaxLength(50)]
        public string? TireSizeRear { get; set; } // Kích thước lốp sau

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public ICollection<StandardMaintenanceSchedule> StandardMaintenanceSchedules { get; set; } = new List<StandardMaintenanceSchedule>();
        public ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
        public ICollection<VehicleVariant> VehicleVariants { get; set; } = new List<VehicleVariant>();
    }

    public enum VehicleFuelType
    {
        [Description("Xăng")]
        Gasoline = 1,

        [Description("Dầu diesel")]
        Diesel = 2,

        [Description("Điện")]
        Electric = 3,

        [Description("Hybrid")]
        Hybrid = 4
    }

    public enum VehicleTransmissionType
    {
        [Description("Xe số")]
        Manual = 1,

        [Description("Tay ga")]
        Automatic = 2,

        [Description("Xe côn")]
        Sport = 3,

        [Description("Số sàn")]
        ManualCar = 4,

        [Description("Số tự động")]
        AutomaticCar = 5,

        [Description("Điện")]
        Electric = 6
    }
}
