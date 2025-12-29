using VMP.Common.UnitOfWork;
using VMP.Identity.Data;
using VMP.Identity.Repositories.Implements;
using VMP.Identity.Repositories.Interfaces;

namespace VMP.Identity.UnitOfWork
{
    public class IdentityUnitOfWork : UnitOfWork<UserDbContext>, IIdentityUnitOfWork
    {
        private IUserRepository? _users;

        public IdentityUnitOfWork(UserDbContext context) : base(context)
        {
        }

        public IUserRepository Users =>
            _users ??= new UserRepository(Context);
    }
}
