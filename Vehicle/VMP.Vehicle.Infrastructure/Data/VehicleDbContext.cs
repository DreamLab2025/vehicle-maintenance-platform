using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Base;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Infrastructure.Data
{
    public class VehicleDbContext : BaseDbContext
    {
        public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
        {
        }

        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleBrand> VehicleBrands { get; set; } = null!;
        public DbSet<VehicleModel> VehicleModels { get; set; } = null!;
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

            modelBuilder.Entity<StandardMaintenanceSchedule>(entity =>
            {
                entity.HasKey(e => new { e.VehicleModelId, e.ConsumableItemId });
                entity.HasQueryFilter(e => e.ConsumableItem.DeletedAt == null);
            });

            modelBuilder.Entity<VehicleType>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleModel>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<ConsumableItem>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceActivity>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceActivityDetail>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<OdometerHistory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserMaintenanceConfig>().HasQueryFilter(e => e.DeletedAt == null);
        }
    }
}
