using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Verender.Common.Databases.Base;
using Verender.Identity.Entities;

namespace Verender.Identity.Data
{
    public class UserDbContext : BaseDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                var roleComparer = new ValueComparer<List<UserRole>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList());

                // Store Roles as array of integers in PostgreSQL
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
