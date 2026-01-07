using VMP.Common.Databases.UnitOfWork;

namespace VMP.Identity.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IUserRepository Users { get; }
    }
}
