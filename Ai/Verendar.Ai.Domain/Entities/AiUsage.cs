namespace Verendar.Ai.Domain.Entities
{
    [Index(nameof(UserId), nameof(CreatedAt))]
    [Index(nameof(Provider), nameof(CreatedAt))]
    public class AiUsage : BaseEntity
    {
        public Guid UserId { get; set; }

        public AiProvider Provider { get; set; }

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = null!;

        public AiOperation Operation { get; set; }

        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int TotalTokens { get; set; }

        [Column(TypeName = "decimal(10, 4)")]
        public decimal InputCost { get; set; }

        [Column(TypeName = "decimal(10, 4)")]
        public decimal OutputCost { get; set; }

        [Column(TypeName = "decimal(10, 4)")]
        public decimal TotalCost { get; set; }

        [MaxLength(500)]
        public string? RequestSummary { get; set; }

        public int ResponseTimeMs { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }
    }

}
