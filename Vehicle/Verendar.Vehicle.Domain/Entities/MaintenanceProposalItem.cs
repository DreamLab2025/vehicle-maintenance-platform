using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(ProposalId))]
    public class MaintenanceProposalItem : BaseEntity
    {
        [Required]
        public Guid ProposalId { get; set; }

        /// <summary>Cross-service ref to Vehicle.PartCategory. Null for pure labor/service items.</summary>
        public Guid? PartCategoryId { get; set; }

        [Required, MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>User can toggle this before applying the proposal.</summary>
        public bool UpdatesTracking { get; set; }

        /// <summary>From GarageProduct.ManufacturerKmInterval — used when opening new TrackingCycle.</summary>
        public int? RecommendedKmInterval { get; set; }

        /// <summary>From GarageProduct.ManufacturerMonthInterval — used when opening new TrackingCycle.</summary>
        public int? RecommendedMonthsInterval { get; set; }

        public decimal Price { get; set; }

        // Navigation
        public MaintenanceProposal Proposal { get; set; } = null!;
    }
}
