namespace Verendar.Ai.Application.Options;

public class PromptVersioningOptions
{
    public const string SectionName = "PromptVersioning";

    public int MaxVersionsPerPrompt { get; set; } = 20;
}
