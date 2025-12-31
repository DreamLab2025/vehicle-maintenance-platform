using System.ComponentModel.DataAnnotations;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class VehicleType : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(500)]
        public string? Description { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
