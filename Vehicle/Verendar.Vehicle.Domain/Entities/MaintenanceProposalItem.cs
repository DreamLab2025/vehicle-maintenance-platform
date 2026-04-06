namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(ProposalId))]
    public class MaintenanceProposalItem : BaseEntity
    {
        [Required]
        public Guid ProposalId { get; set; }

        public Guid? PartCategoryId { get; set; }

        public Guid? GarageProductId { get; set; }

        public Guid? GarageServiceId { get; set; }

        [Required, MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public bool UpdatesTracking { get; set; }

        public int? RecommendedKmInterval { get; set; }

        public int? RecommendedMonthsInterval { get; set; }

        public decimal Price { get; set; }

        // Navigation
        public MaintenanceProposal Proposal { get; set; } = null!;
    }
}
