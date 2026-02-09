using Verendar.Common.Databases.Implements;
using Verendar.Identity.Domain.Entities;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verendar.Identity.Infrastructure.Data;

namespace Verendar.Identity.Infrastructure.Repositories.Implements;

public class UserRepository(UserDbContext context) : PostgresRepository<User>(context), IUserRepository
{
}
