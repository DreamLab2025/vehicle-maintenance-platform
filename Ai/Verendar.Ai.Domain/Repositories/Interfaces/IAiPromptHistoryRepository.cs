using Verendar.Ai.Domain.Entities;

namespace Verendar.Ai.Domain.Repositories.Interfaces;

public interface IAiPromptHistoryRepository : IGenericRepository<AiPromptHistory>
{
    Task<List<AiPromptHistory>> GetByPromptIdAsync(Guid aiPromptId, CancellationToken cancellationToken = default);
    Task<AiPromptHistory?> GetByVersionAsync(Guid aiPromptId, int versionNumber, CancellationToken cancellationToken = default);

    Task<int> RemoveExcessVersionsAsync(Guid aiPromptId, int maxVersionsToKeep, CancellationToken cancellationToken = default);
}
