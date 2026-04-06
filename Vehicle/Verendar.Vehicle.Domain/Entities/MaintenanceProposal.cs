namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(UserVehicleId))]
    [Index(nameof(BookingId), IsUnique = true)]
    public class MaintenanceProposal : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BookingId { get; set; }

        public Guid GarageBranchId { get; set; }

        [MaxLength(200)]
        public string BranchName { get; set; } = string.Empty;

        public DateOnly ServiceDate { get; set; }

        public int? OdometerAtService { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public decimal TotalAmount { get; set; }

        public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

        public DateTime? AppliedAt { get; set; }

        public Guid? ResultMaintenanceRecordId { get; set; }

        // Navigation
        public UserVehicle UserVehicle { get; set; } = null!;
        public List<MaintenanceProposalItem> Items { get; set; } = [];
    }
}
