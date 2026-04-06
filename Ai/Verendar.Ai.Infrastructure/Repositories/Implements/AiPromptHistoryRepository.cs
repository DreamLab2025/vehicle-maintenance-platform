using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Common.Databases.Implements;

namespace Verendar.Ai.Infrastructure.Repositories.Implements;

public class AiPromptHistoryRepository(AiDbContext context) : PostgresRepository<AiPromptHistory>(context), IAiPromptHistoryRepository
{
    public async Task<List<AiPromptHistory>> GetByPromptIdAsync(Guid aiPromptId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.AiPromptId == aiPromptId)
            .OrderByDescending(h => h.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<AiPromptHistory?> GetByVersionAsync(Guid aiPromptId, int versionNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.AiPromptId == aiPromptId && h.VersionNumber == versionNumber, cancellationToken);
    }

    public async Task<int> RemoveExcessVersionsAsync(
        Guid aiPromptId,
        int maxVersionsToKeep,
        CancellationToken cancellationToken = default)
    {
        var expired = await _dbSet
            .Where(h => h.AiPromptId == aiPromptId)
            .OrderByDescending(h => h.VersionNumber)
            .Skip(maxVersionsToKeep)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return 0;

        _dbSet.RemoveRange(expired);
        return expired.Count;
    }
}
