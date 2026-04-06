using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Infrastructure.Repositories.Implements
{
    public class UserRepository(UserDbContext context) : PostgresRepository<User>(context), IUserRepository
    {
    }
}
