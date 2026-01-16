using Microsoft.EntityFrameworkCore;
using Verender.Common.Databases.Base;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Infrastructure.Data
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
        public DbSet<ConsumableItem> ConsumableItems { get; set; } = null!;
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
                entity.HasIndex(e => new { e.VehicleModelId, e.ConsumableItemId })
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

            modelBuilder.Entity<VehicleType>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleTypeBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleModel>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleVariant>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<ConsumableItem>().HasQueryFilter(e => e.DeletedAt == null);
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
    }
}
