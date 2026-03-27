namespace Verendar.Ai.Infrastructure.Configuration;

public class PromptVersioningOptions
{
    public const string SectionName = "PromptVersioning";

    public int MaxVersionsPerPrompt { get; set; } = 20;
}
