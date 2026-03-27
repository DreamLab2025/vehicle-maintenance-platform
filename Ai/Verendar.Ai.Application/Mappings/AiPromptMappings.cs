using Verendar.Ai.Application.Dtos.AiPrompt;
using Verendar.Ai.Domain.Entities;

namespace Verendar.Ai.Application.Mappings;

public static class AiPromptMappings
{
    public static AiPromptResponse ToResponse(this AiPrompt entity) => new()
    {
        Id = entity.Id,
        Operation = (int)entity.Operation,
        OperationName = entity.Operation.ToString(),
        Provider = (int)entity.Provider,
        ProviderName = entity.Provider.ToString(),
        Name = entity.Name,
        Description = entity.Description,
        Content = entity.Content,
        VersionNumber = entity.VersionNumber,
        UpdatedAt = entity.UpdatedAt,
    };

    public static AiPromptVersionResponse ToVersionResponse(this AiPromptHistory history, int currentVersion) => new()
    {
        VersionNumber = history.VersionNumber,
        Provider = (int)history.Provider,
        ProviderName = history.Provider.ToString(),
        Content = history.Content,
        Note = history.Note,
        CreatedAt = history.CreatedAt,
        IsCurrent = history.VersionNumber == currentVersion,
    };
}
