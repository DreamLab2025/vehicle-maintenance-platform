using Verendar.Ai.Domain.Repositories.Interfaces;
namespace Verendar.Ai.Infrastructure.Repositories.Implements
{
    public class AiUsageRepository(AiDbContext context) : PostgresRepository<AiUsage>(context), IAiUsageRepository
    {
    }
}
