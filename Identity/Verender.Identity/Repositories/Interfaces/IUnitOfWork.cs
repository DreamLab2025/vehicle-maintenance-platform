using Verender.Common.Databases.UnitOfWork;

namespace Verender.Identity.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IUserRepository Users { get; }
    }
}
