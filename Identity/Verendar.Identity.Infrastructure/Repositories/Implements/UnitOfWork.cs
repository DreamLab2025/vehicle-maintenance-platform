using Verendar.Common.Databases.UnitOfWork;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verendar.Identity.Infrastructure.Data;

namespace Verendar.Identity.Infrastructure.Repositories.Implements;

public class UnitOfWork(UserDbContext context) : BaseUnitOfWork<UserDbContext>(context), IUnitOfWork
{
    private IUserRepository? _users;

    public IUserRepository Users =>
        _users ??= new UserRepository(Context);
}
