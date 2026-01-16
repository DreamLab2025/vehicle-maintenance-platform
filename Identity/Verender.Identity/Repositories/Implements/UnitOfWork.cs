using Verender.Common.Databases.UnitOfWork;
using Verender.Identity.Data;
using Verender.Identity.Repositories.Interfaces;

namespace Verender.Identity.Repositories.Implements
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
