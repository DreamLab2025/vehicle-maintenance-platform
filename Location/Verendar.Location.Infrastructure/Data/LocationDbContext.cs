namespace Verendar.Location.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

public class LocationDbContext(DbContextOptions<LocationDbContext> options) : DbContext(options)
{
    public DbSet<AdministrativeRegion> AdministrativeRegions { get; set; } = null!;
    public DbSet<AdministrativeUnit> AdministrativeUnits { get; set; } = null!;
    public DbSet<Province> Provinces { get; set; } = null!;
    public DbSet<Ward> Wards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdministrativeRegion configuration
        modelBuilder.Entity<AdministrativeRegion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        // AdministrativeUnit configuration
        modelBuilder.Entity<AdministrativeUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Abbreviation).HasMaxLength(20);
        });

        // Province configuration (string PK: Code)
        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();

            // Foreign key to AdministrativeRegion
            entity.HasOne(e => e.AdministrativeRegion)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.AdministrativeRegionId);
            entity.HasIndex(e => e.Name);
        });

        // Ward configuration (string PK: Code)
        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ProvinceCode).HasMaxLength(10);

            // Foreign key to Province (by code)
            entity.HasOne(e => e.Province)
                .WithMany(p => p.Wards)
                .HasForeignKey(e => e.ProvinceCode)
                .HasPrincipalKey(p => p.Code)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to AdministrativeUnit
            entity.HasOne(e => e.AdministrativeUnit)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.ProvinceCode);
            entity.HasIndex(e => e.AdministrativeUnitId);
            entity.HasIndex(e => e.Name);
        });
    }
}
