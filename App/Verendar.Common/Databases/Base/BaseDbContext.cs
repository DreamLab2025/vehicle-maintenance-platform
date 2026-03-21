using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Interfaces;
using Verendar.Common.Jwt;

namespace Verendar.Common.Databases.Base
{
    public abstract class BaseDbContext(DbContextOptions options, ICurrentUserService? currentUserService = null) : DbContext(options)
    {
        private readonly ICurrentUserService? _currentUserService = currentUserService;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>();
            var actingUserId = _currentUserService?.UserId ?? Guid.Empty;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (entry.Entity.CreatedBy == Guid.Empty)
                        entry.Entity.CreatedBy = actingUserId != Guid.Empty ? actingUserId : entry.Entity.Id;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    if (entry.Entity.UpdatedBy == null || entry.Entity.UpdatedBy == Guid.Empty)
                        entry.Entity.UpdatedBy = actingUserId != Guid.Empty ? actingUserId : entry.Entity.Id;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
