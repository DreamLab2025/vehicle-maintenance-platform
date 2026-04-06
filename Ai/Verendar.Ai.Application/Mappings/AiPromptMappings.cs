using Verendar.Ai.Application.Dtos.AiPrompt;

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

    public static AiPromptVersionResponse ToVersionResponse(this AiPromptHistory history, bool isCurrent) => new()
    {
        VersionNumber = history.VersionNumber,
        Provider = (int)history.Provider,
        ProviderName = history.Provider.ToString(),
        Content = history.Content,
        Note = history.Note,
        CreatedAt = history.CreatedAt,
        IsCurrent = isCurrent,
    };

    public static AiPromptVersionResponse ToCurrentVersionResponse(this AiPrompt entity) => new()
    {
        VersionNumber = entity.VersionNumber,
        Provider = (int)entity.Provider,
        ProviderName = entity.Provider.ToString(),
        Content = entity.Content,
        Note = null,
        CreatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        IsCurrent = true,
    };
}
