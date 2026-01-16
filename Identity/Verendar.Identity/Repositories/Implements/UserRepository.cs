using Verendar.Common.Databases.Implements;
using Verendar.Identity.Data;
using Verendar.Identity.Entities;
using Verendar.Identity.Repositories.Interfaces;

namespace Verendar.Identity.Repositories.Implements
{
    public class UserRepository : PostgresRepository<User>, IUserRepository
    {
        public UserRepository(UserDbContext context) : base(context)
        {
        }
    }
}
