using Microsoft.EntityFrameworkCore;
using Verendar.Ai.Domain.Entities;
using Verendar.Common.Databases.Base;
using Verendar.Common.Jwt;

namespace Verendar.Ai.Infrastructure.Data
{
    public class AiDbContext(DbContextOptions<AiDbContext> options, ICurrentUserService? currentUserService = null) : BaseDbContext(options, currentUserService)
    {
        public DbSet<AiUsage> AiUsages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
