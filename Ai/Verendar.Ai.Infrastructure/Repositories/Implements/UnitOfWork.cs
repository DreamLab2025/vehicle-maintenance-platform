using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Common.Databases.UnitOfWork;


namespace Verendar.Ai.Infrastructure.Repositories.Implements;

public class UnitOfWork(AiDbContext context) : BaseUnitOfWork<AiDbContext>(context), IUnitOfWork
{
    private IAiUsageRepository? _aiUsages;

    public IAiUsageRepository AiUsages => _aiUsages ??= new AiUsageRepository(Context);
}
