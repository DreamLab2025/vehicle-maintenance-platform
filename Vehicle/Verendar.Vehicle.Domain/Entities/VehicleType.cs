using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class VehicleType : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public List<VehicleBrand> Brands { get; set; } = [];
    }
}
