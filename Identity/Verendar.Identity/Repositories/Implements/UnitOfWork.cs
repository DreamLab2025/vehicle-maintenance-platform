using Verendar.Common.Databases.UnitOfWork;
using Verendar.Identity.Data;
using Verendar.Identity.Repositories.Interfaces;

namespace Verendar.Identity.Repositories.Implements
{
    public class UnitOfWork(UserDbContext context) : BaseUnitOfWork<UserDbContext>(context), IUnitOfWork
    {
        private IUserRepository? _users;

        public IUserRepository Users =>
            _users ??= new UserRepository(Context);
    }
}
