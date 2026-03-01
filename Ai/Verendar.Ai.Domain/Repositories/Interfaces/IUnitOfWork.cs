using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Ai.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IAiUsageRepository AiUsages { get; }
    }
}
