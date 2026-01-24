using Verendar.Common.Databases.UnitOfWork;
using Verendar.Identity.Data;
using Verendar.Identity.Repositories.Interfaces;

namespace Verendar.Identity.Repositories.Implements
{
    public class UnitOfWork : BaseUnitOfWork<UserDbContext>, IUnitOfWork
    {
        private IUserRepository? _users;

        public UnitOfWork(UserDbContext context) : base(context)
        {
        }

        public IUserRepository Users =>
            _users ??= new UserRepository(Context);
    }
}
