using System.ComponentModel.DataAnnotations;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class VehicleType : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
