namespace Verendar.Ai.Application.Dtos.AiPrompt;

public class AiPromptResponse
{
    public Guid Id { get; set; }
    public int Operation { get; set; }
    public string OperationName { get; set; } = null!;
    public int Provider { get; set; }
    public string ProviderName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Content { get; set; } = null!;
    public int VersionNumber { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
