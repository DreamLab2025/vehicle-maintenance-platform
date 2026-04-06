using Verendar.Ai.Domain.Entities;

namespace Verendar.Ai.Domain.Repositories.Interfaces;

public interface IAiPromptRepository : IGenericRepository<AiPrompt>
{
    Task<AiPrompt?> GetByOperationAsync(AiOperation operation, CancellationToken cancellationToken = default);
    Task<List<AiPrompt>> GetAllPromptsAsync(CancellationToken cancellationToken = default);
}
