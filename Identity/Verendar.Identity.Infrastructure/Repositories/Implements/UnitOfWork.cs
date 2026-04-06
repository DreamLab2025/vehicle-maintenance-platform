using Verendar.Common.Databases.Interfaces;
using Verendar.Common.Databases.UnitOfWork;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Infrastructure.Repositories.Implements
{
    public class UnitOfWork(UserDbContext context) : BaseUnitOfWork<UserDbContext>(context), IUnitOfWork
    {
        private IUserRepository? _users;
        private IGenericRepository<Feedback>? _feedbacks;
        private IIdentityStatsRepository? _stats;

        public IUserRepository Users =>
            _users ??= new UserRepository(Context);

        public IGenericRepository<Feedback> Feedbacks =>
            _feedbacks ??= new PostgresRepository<Feedback>(Context);

        public IIdentityStatsRepository Stats =>
            _stats ??= new IdentityStatsRepository(Context);
    }
}
