using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Identity.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository Users { get; }
}
