using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class VehicleBrand : BaseEntity
    {
        [Required]
        public Guid VehicleTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(20)]
        public string? SupportPhone { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public VehicleType VehicleType { get; set; } = null!;
        public List<VehicleModel> VehicleModels { get; set; } = [];
    }
}
