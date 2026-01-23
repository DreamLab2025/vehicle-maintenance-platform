using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    [Table("VehiclePartCategories")]
    public class VehiclePartCategory : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Code { get; set; } = null!; // Unique code: OIL, TIRES, BATTERY, etc.

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? IconUrl { get; set; }

        /// Thứ tự hiển thị (sắp xếp)
        public int DisplayOrder { get; set; } = 0;

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public ICollection<VehiclePart> VehicleParts { get; set; } = new List<VehiclePart>();
    }

    /// Predefined part categories codes
    public static class VehiclePartCategoryCodes
    {
        public const string Oil = "OIL";
        public const string Tires = "TIRES";
        public const string Battery = "BATTERY";
        public const string AirFilter = "AIR_FILTER";
        public const string OilFilter = "OIL_FILTER";
        public const string BrakePad = "BRAKE_PAD";
        public const string BrakeFluid = "BRAKE_FLUID";
        public const string Coolant = "COOLANT";
        public const string SparkPlug = "SPARK_PLUG";
        public const string WiperBlade = "WIPER_BLADE";
        public const string Belt = "BELT";
        public const string Other = "OTHER";
    }
}
