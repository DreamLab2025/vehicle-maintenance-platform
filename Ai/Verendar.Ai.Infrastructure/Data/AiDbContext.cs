using System;
using Microsoft.EntityFrameworkCore;
using Verendar.Ai.Domain.Entities;
using Verendar.Common.Databases.Base;

namespace Verendar.Ai.Infrastructure.Data;

public class AiDbContext : BaseDbContext
{
    public DbSet<AiUsage> AiUsages { get; set; } = null!;
    public AiDbContext(DbContextOptions<AiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
