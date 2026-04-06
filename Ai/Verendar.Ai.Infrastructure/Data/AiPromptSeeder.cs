using System.Reflection;
using Verendar.Ai.Domain.Enums;

namespace Verendar.Ai.Infrastructure.Data;

public static class AiPromptSeeder
{
    private static readonly (AiOperation Operation, AiProvider Provider, string Name, string Description, string ResourceName)[] PromptDefinitions =
    [
        (AiOperation.ReadOdometerFromImage, AiProvider.Gemini,
            "Odometer Scan Prompt",
            "English instructions; user-facing message in JSON must be Vietnamese.",
            "Verendar.Ai.Infrastructure.Data.Prompts.odometer-scan.txt"),

        (AiOperation.AnalyzeMaintenanceQuestionnaire, AiProvider.Bedrock,
            "Vehicle Maintenance Analysis Prompt",
            "English instructions; reasoning and warnings in JSON must be Vietnamese.",
            "Verendar.Ai.Infrastructure.Data.Prompts.analyze-maintenance-questionnaire.txt"),
    ];

    public static async Task SeedAsync(AiDbContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var (operation, provider, name, description, resourceName) in PromptDefinitions)
        {
            var exists = await context.AiPrompts.AnyAsync(p => p.Operation == operation);
            if (exists)
                continue;

            var content = await ReadEmbeddedResourceAsync(assembly, resourceName);

            var prompt = new AiPrompt
            {
                Operation = operation,
                Provider = provider,
                Name = name,
                Description = description,
                Content = content,
                VersionNumber = 1,
            };

            await context.AiPrompts.AddAsync(prompt);
        }

        await context.SaveChangesAsync();
    }

    private static async Task<string> ReadEmbeddedResourceAsync(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
