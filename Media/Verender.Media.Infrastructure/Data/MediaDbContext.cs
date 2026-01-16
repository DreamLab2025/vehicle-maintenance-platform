using Microsoft.EntityFrameworkCore;
using Verender.Common.Databases.Base;

namespace Verender.Media.Infrastructure.Data
{
    public class MediaDbContext : BaseDbContext
    {
        public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
        {
        }

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
