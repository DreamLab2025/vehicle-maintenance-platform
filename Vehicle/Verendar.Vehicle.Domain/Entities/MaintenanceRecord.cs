namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(UserVehicleId), nameof(ServiceDate))]
    public class MaintenanceRecord : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }

        public int OdometerAtService { get; set; }

        [MaxLength(200)]
        public string? GarageName { get; set; }

        public decimal TotalCost { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? InvoiceImageUrl { get; set; }

        public UserVehicle UserVehicle { get; set; } = null!;

        public List<MaintenanceRecordItem> Items { get; set; } = [];
    }
}
