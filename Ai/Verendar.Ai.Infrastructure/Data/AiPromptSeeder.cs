using System.Reflection;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;

namespace Verendar.Ai.Infrastructure.Data;

public static class AiPromptSeeder
{
    private static readonly (AiOperation Operation, AiProvider Provider, string Name, string Description, string ResourceName)[] PromptDefinitions =
    [
        (AiOperation.ReadOdometerFromImage, AiProvider.Gemini,
            "Odometer Scan Prompt",
            "Prompt hướng dẫn AI đọc số km từ ảnh đồng hồ công-tơ-mét",
            "Verendar.Ai.Infrastructure.Prompts.odometer-scan.txt"),

        (AiOperation.AnalyzeMaintenanceQuestionnaire, AiProvider.Gemini,
            "Vehicle Maintenance Analysis Prompt",
            "Prompt phân tích câu hỏi bảo dưỡng xe và đưa ra khuyến nghị dựa trên Q&A và lịch hãng",
            "Verendar.Ai.Infrastructure.Prompts.analyze-maintenance-questionnaire.txt"),
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

            var history = new AiPromptHistory
            {
                AiPromptId = prompt.Id,
                VersionNumber = 1,
                Provider = provider,
                Content = content,
                Note = "Initial seed",
            };

            await context.AiPrompts.AddAsync(prompt);
            await context.AiPromptHistories.AddAsync(history);
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
