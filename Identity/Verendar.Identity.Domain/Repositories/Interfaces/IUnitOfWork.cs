using Verendar.Common.Databases.UnitOfWork;
using Verendar.Identity.Domain.Entities;

namespace Verendar.Identity.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IUserRepository Users { get; }
        IGenericRepository<Feedback> Feedbacks { get; }
        IIdentityStatsRepository Stats { get; }
    }
}
