using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class VehicleTypeBrand : BaseEntity
    {
        [Required]
        public Guid VehicleTypeId { get; set; }
        [ForeignKey(nameof(VehicleTypeId))]
        public VehicleType VehicleType { get; set; } = null!;

        [Required]
        public Guid VehicleBrandId { get; set; }
        [ForeignKey(nameof(VehicleBrandId))]
        public VehicleBrand VehicleBrand { get; set; } = null!;

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
