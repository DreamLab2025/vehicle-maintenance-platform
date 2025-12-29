using VMP.Common.Databases.Implements;
using VMP.Identity.Data;
using VMP.Identity.Entities;
using VMP.Identity.Repositories.Interfaces;

namespace VMP.Identity.Repositories.Implements
{
    public class UserRepository : PostgresRepository<User>, IUserRepository
    {
        public UserRepository(UserDbContext context) : base(context)
        {
        }
    }
}
