using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Common.Databases.Implements;

namespace Verendar.Ai.Infrastructure.Repositories.Implements
{
    public class AiUsageRepository(AiDbContext context) : PostgresRepository<AiUsage>(context), IAiUsageRepository
    {
    }
}
