namespace Verendar.Ai.Domain.Entities;

[Index(nameof(AiPromptId), nameof(VersionNumber), IsUnique = true)]
public class AiPromptHistory : IEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid AiPromptId { get; set; }

    public int VersionNumber { get; set; }

    public AiProvider Provider { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiPrompt AiPrompt { get; set; } = null!;
}
