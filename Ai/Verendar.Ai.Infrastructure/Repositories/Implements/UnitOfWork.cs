using System;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Common.Databases.UnitOfWork;


namespace Verendar.Ai.Infrastructure.Repositories.Implements;

public class UnitOfWork : BaseUnitOfWork<AiDbContext>, IUnitOfWork
{
    private IAiUsageRepository? _aiUsages;

    public UnitOfWork(AiDbContext context) : base(context)
    {
    }

    public IAiUsageRepository AiUsages => _aiUsages ??= new AiUsageRepository(Context);
}
