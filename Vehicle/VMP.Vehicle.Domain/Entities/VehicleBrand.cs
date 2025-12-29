using System.ComponentModel.DataAnnotations;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class VehicleBrand : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(20)]
        public string? SupportPhone { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
