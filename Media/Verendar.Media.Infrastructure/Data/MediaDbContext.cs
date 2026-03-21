using Verendar.Common.Databases.Base;
using Verendar.Common.Jwt;

namespace Verendar.Media.Infrastructure.Data
{
    public class MediaDbContext(DbContextOptions<MediaDbContext> options, ICurrentUserService? currentUserService = null) : BaseDbContext(options, currentUserService)
    {
        public DbSet<Domain.Entities.MediaFile> Files { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Domain.Entities.MediaFile>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
