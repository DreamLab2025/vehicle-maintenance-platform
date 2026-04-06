namespace Verendar.Ai.Application.Dtos.AiPrompt;

public class AiPromptVersionResponse
{
    public int VersionNumber { get; set; }
    public int Provider { get; set; }
    public string ProviderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCurrent { get; set; }
}
