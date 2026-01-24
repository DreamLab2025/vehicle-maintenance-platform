using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Identity.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IUserRepository Users { get; }
    }
}
