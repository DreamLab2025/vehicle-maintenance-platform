using Verender.Common.Databases.Implements;
using Verender.Identity.Data;
using Verender.Identity.Entities;
using Verender.Identity.Repositories.Interfaces;

namespace Verender.Identity.Repositories.Implements
{
    public class UserRepository : PostgresRepository<User>, IUserRepository
    {
        public UserRepository(UserDbContext context) : base(context)
        {
        }
    }
}
