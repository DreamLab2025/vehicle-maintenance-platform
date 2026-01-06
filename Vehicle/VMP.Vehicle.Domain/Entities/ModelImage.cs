using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class ModelImage : BaseEntity
    {
        [Required]
        public Guid VehicleModelId { get; set; }
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;
    }
}
