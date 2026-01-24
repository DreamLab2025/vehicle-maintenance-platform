using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

namespace Verendar.Media.Infrastructure.Data
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
