using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class VehicleModel : BaseEntity
    {
        [Required]
        public Guid VehicleBrandId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        public int? ManufactureYear { get; set; }

        public VehicleFuelType? FuelType { get; set; }

        public VehicleTransmissionType? TransmissionType { get; set; }

        public int? EngineDisplacement { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? EngineCapacity { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public VehicleBrand Brand { get; set; } = null!;
        public List<VehicleVariant> Variants { get; set; } = [];
        public List<DefaultMaintenanceSchedule> DefaultSchedules { get; set; } = [];
    }

    public enum VehicleFuelType
    {
        [Description("Xăng")]
        Gasoline = 1,

        [Description("Dầu diesel")]
        Diesel = 2,

        [Description("Hybrid")]
        Hybrid = 3
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
        AutomaticCar = 5
    }
}
