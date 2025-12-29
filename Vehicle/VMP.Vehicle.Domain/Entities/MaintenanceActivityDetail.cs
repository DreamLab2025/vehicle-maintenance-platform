using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class MaintenanceActivityDetail : BaseEntity
    {
        [Required]
        public Guid MaintenanceActivityId { get; set; }
        [ForeignKey(nameof(MaintenanceActivityId))]
        public MaintenanceActivity MaintenanceActivity { get; set; } = null!;

        [Required]
        public Guid ConsumableItemId { get; set; }
        [ForeignKey(nameof(ConsumableItemId))]
        public ConsumableItem ConsumableItem { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
