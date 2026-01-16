using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verender.Common.Databases.Base;

namespace Verender.Vehicle.Domain.Entities
{
    public class VehicleVariant : BaseEntity
    {
        [Required]
        public Guid VehicleModelId { get; set; }
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        [MaxLength(7)]
        public string HexCode { get; set; } = null!;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;
    }
}
