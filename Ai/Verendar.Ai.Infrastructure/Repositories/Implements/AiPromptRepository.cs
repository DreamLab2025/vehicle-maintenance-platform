using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Common.Databases.Implements;

namespace Verendar.Ai.Infrastructure.Repositories.Implements;

public class AiPromptRepository(AiDbContext context) : PostgresRepository<AiPrompt>(context), IAiPromptRepository
{
    public async Task<AiPrompt?> GetByOperationAsync(AiOperation operation, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.DeletedAt == null)
            .FirstOrDefaultAsync(p => p.Operation == operation, cancellationToken);
    }

    public async Task<List<AiPrompt>> GetAllPromptsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.DeletedAt == null)
            .OrderBy(p => p.Operation)
            .ToListAsync(cancellationToken);
    }
}
