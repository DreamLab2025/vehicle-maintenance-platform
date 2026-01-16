using System.ComponentModel.DataAnnotations;
using Verender.Common.Databases.Base;

namespace Verender.Vehicle.Domain.Entities
{
    public class ConsumableItem : BaseEntity
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
