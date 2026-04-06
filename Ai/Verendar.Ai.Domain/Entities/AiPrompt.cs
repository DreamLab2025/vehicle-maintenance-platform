namespace Verendar.Ai.Domain.Entities;

[Index(nameof(Operation), IsUnique = true)]
public class AiPrompt : BaseEntity
{
    public AiOperation Operation { get; set; }

    public AiProvider Provider { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    public int VersionNumber { get; set; } = 1;

    public ICollection<AiPromptHistory> Histories { get; set; } = new List<AiPromptHistory>();
}
