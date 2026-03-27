namespace Verendar.Ai.Application.Dtos.AiPrompt;

public class RollbackAiPromptRequest
{
    public int VersionNumber { get; set; }
    public string? Note { get; set; }
}
