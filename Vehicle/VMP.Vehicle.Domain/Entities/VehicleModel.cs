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

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

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
    }

    public enum VehicleFuelType
    {
        Gasoline = 1,
        Diesel = 2,
        Electric = 3,
        Hybrid = 4
    }

    public enum VehicleTransmissionType
    {
        Manual = 1,      // Xe số / Số sàn
        Automatic = 2,   // Tay ga / Số tự động
        Electric = 3     // Điện (không hộp số)
    }
}
