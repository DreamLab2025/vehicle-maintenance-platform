using System.ComponentModel.DataAnnotations;
using Verender.Common.Databases.Base;

namespace Verender.Vehicle.Domain.Entities
{
    public class VehicleType : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public ICollection<VehicleTypeBrand> VehicleTypeBrands { get; set; } = new List<VehicleTypeBrand>();
        public ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
    }
}
