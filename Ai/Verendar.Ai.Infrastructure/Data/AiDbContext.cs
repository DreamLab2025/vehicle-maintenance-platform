using Microsoft.EntityFrameworkCore;
using Verendar.Ai.Domain.Entities;
using Verendar.Common.Databases.Base;

namespace Verendar.Ai.Infrastructure.Data;

public class AiDbContext(DbContextOptions<AiDbContext> options) : BaseDbContext(options)
{
    public DbSet<AiUsage> AiUsages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
