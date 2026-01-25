using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceRecord : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }

        public int OdometerAtService { get; set; }

        [MaxLength(200)]
        public string? GarageName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? InvoiceImageUrl { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public UserVehicle UserVehicle { get; set; } = null!;

        public List<MaintenanceRecordItem> Items { get; set; } = [];
    }
}
