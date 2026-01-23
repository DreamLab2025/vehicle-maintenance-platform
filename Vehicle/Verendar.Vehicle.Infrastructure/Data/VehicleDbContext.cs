using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Infrastructure.Data
{
    public class VehicleDbContext : BaseDbContext
    {
        public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
        {
        }

        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleBrand> VehicleBrands { get; set; } = null!;
        public DbSet<VehicleTypeBrand> VehicleTypeBrands { get; set; } = null!;
        public DbSet<VehicleModel> VehicleModels { get; set; } = null!;
        public DbSet<VehicleVariant> VehicleVariants { get; set; } = null!;
        public DbSet<UserVehicle> UserVehicles { get; set; } = null!;
        public DbSet<VehiclePart> VehicleParts { get; set; } = null!;
        public DbSet<VehiclePartCategory> VehiclePartCategories { get; set; } = null!;
        public DbSet<Oil> Oils { get; set; } = null!;
        public DbSet<MaintenanceActivity> MaintenanceActivities { get; set; } = null!;
        public DbSet<MaintenanceActivityDetail> MaintenanceActivityDetails { get; set; } = null!;
        public DbSet<OdometerHistory> OdometerHistories { get; set; } = null!;
        public DbSet<StandardMaintenanceSchedule> StandardMaintenanceSchedules { get; set; } = null!;
        public DbSet<UserMaintenanceConfig> UserMaintenanceConfigs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VehicleTypeBrand>(entity =>
            {
                entity.Ignore(e => e.Id);
                entity.HasKey(e => new { e.VehicleTypeId, e.VehicleBrandId });

                entity.HasOne(e => e.VehicleType)
                    .WithMany(vt => vt.VehicleTypeBrands)
                    .HasForeignKey(e => e.VehicleTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.VehicleBrand)
                    .WithMany(vb => vb.VehicleTypeBrands)
                    .HasForeignKey(e => e.VehicleBrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StandardMaintenanceSchedule>(entity =>
            {
                entity.HasIndex(e => new { e.VehicleModelId, e.VehiclePartId })
                    .IsUnique();
            });

            modelBuilder.Entity<VehicleVariant>(entity =>
            {
                entity.HasIndex(e => new { e.VehicleModelId, e.Color });

                entity.HasOne(e => e.VehicleModel)
                    .WithMany(vm => vm.VehicleVariants)
                    .HasForeignKey(e => e.VehicleModelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Oil entity configuration
            modelBuilder.Entity<Oil>(entity =>
            {
                entity.HasOne(o => o.VehiclePart)
                    .WithOne()
                    .HasForeignKey<Oil>(o => o.VehiclePartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // VehiclePart entity configuration - filtered indexes
            modelBuilder.Entity<VehiclePart>(entity =>
            {
                entity.HasIndex(p => p.Name)
                    .HasFilter("\"DeletedAt\" IS NULL");

                entity.HasIndex(p => p.Sku)
                    .HasFilter("\"DeletedAt\" IS NULL AND \"Sku\" IS NOT NULL");

                entity.HasIndex(p => new { p.CategoryId, p.Status })
                    .HasFilter("\"DeletedAt\" IS NULL");

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.VehicleParts)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // VehiclePartCategory entity configuration - filtered indexes
            modelBuilder.Entity<VehiclePartCategory>(entity =>
            {
                entity.HasIndex(c => c.Code)
                    .IsUnique()
                    .HasFilter("\"DeletedAt\" IS NULL")
                    .HasDatabaseName("IX_VehiclePartCategories_Code_Unique");

                entity.HasIndex(c => new { c.DisplayOrder, c.Status })
                    .HasFilter("\"DeletedAt\" IS NULL");

                entity.HasMany(c => c.VehicleParts)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VehicleType>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleTypeBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleModel>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleVariant>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehiclePart>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehiclePartCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Oil>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceActivity>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceActivityDetail>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<OdometerHistory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<StandardMaintenanceSchedule>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserMaintenanceConfig>().HasQueryFilter(e => e.DeletedAt == null);
            

            // Seed data
            SeedVehicleTypes(modelBuilder);
           
        }

        private void SeedVehicleTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleType>().HasData(VehicleDataSeeder.GetVehicleTypes());
        }

        // TODO: Implement VehiclePartCategory seeder
        // private void SeedVehiclePartCategories(ModelBuilder modelBuilder)
        // {
        //     modelBuilder.Entity<VehiclePartCategory>().HasData(
        //         Seeders.VehiclePartCategorySeeder.GetVehiclePartCategories());
        // }
    }
}
