using VMP.Identity.Repositories.Interfaces;

namespace VMP.Identity.UnitOfWork
{
    public interface IIdentityUnitOfWork : Common.UnitOfWork.IUnitOfWork
    {
        IUserRepository Users { get; }
    }
}
