namespace Verendar.Ai.Application.Dtos.AiPrompt;

public class UpdateAiPromptRequest
{
    public string Content { get; set; } = null!;
    public int Provider { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Note { get; set; }
}
