using VMP.Common.Databases.UnitOfWork;
using VMP.Identity.Data;
using VMP.Identity.Repositories.Interfaces;

namespace VMP.Identity.Repositories.Implements
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
