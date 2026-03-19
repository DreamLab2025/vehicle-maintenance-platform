using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Verendar.Common.Databases.Base;
using Verendar.Common.Jwt;
using Verendar.Identity.Domain.Entities;

namespace Verendar.Identity.Infrastructure.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options, ICurrentUserService? currentUserService = null) : BaseDbContext(options, currentUserService)
    {
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                var roleComparer = new ValueComparer<List<UserRole>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList());

                entity.Property(e => e.Roles)
                    .HasConversion(
                        v => v.Select(r => (int)r).ToArray(),
                        v => v.Select(r => (UserRole)r).ToList())
                    .HasColumnType("integer[]")
                    .Metadata.SetValueComparer(roleComparer);

                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
