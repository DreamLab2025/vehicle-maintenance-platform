using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(UserVehicleId))]
    [Index(nameof(BookingId), IsUnique = true)]
    public class MaintenanceProposal : BaseEntity
    {
        /// <summary>Cross-service ref to UserVehicle (Vehicle service owns this).</summary>
        [Required]
        public Guid UserVehicleId { get; set; }

        /// <summary>Cross-service ref to Identity user (vehicle owner).</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>Cross-service ref to Garage.Booking — unique constraint prevents duplicate proposals.</summary>
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

        /// <summary>Set after apply — references the created MaintenanceRecord.</summary>
        public Guid? ResultMaintenanceRecordId { get; set; }

        // Navigation
        public UserVehicle UserVehicle { get; set; } = null!;
        public List<MaintenanceProposalItem> Items { get; set; } = [];
    }
}
