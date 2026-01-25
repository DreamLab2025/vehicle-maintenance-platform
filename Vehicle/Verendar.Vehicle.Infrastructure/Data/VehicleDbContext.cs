using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Infrastructure.Data
{
    public class VehicleDbContext(DbContextOptions<VehicleDbContext> options) : BaseDbContext(options)
    {

        // Vehicle Catalog
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleBrand> VehicleBrands { get; set; } = null!;
        public DbSet<VehicleModel> VehicleModels { get; set; } = null!;
        public DbSet<VehicleVariant> VehicleVariants { get; set; } = null!;

        // Part Catalog
        public DbSet<PartCategory> PartCategories { get; set; } = null!;
        public DbSet<PartProduct> PartProducts { get; set; } = null!;

        // Maintenance Schedule
        public DbSet<DefaultMaintenanceSchedule> DefaultMaintenanceSchedules { get; set; } = null!;

        // User Vehicle & Tracking
        public DbSet<UserVehicle> UserVehicles { get; set; } = null!;
        public DbSet<OdometerHistory> OdometerHistories { get; set; } = null!;
        public DbSet<VehiclePartTracking> VehiclePartTrackings { get; set; } = null!;
        public DbSet<MaintenanceReminder> MaintenanceReminders { get; set; } = null!;

        // Maintenance History
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = null!;
        public DbSet<MaintenanceRecordItem> MaintenanceRecordItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============== INDEXES ===============
            // Unique indexes
            modelBuilder.Entity<VehicleType>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<VehicleBrand>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<VehicleModel>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartCategory>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<DefaultMaintenanceSchedule>().HasIndex(e => new { e.VehicleModelId, e.PartCategoryId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<VehiclePartTracking>().HasIndex(e => new { e.UserVehicleId, e.PartCategoryId, e.InstanceIdentifier }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

            // Performance indexes
            modelBuilder.Entity<VehicleModel>().HasIndex(e => new { e.VehicleBrandId, e.Status }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartCategory>().HasIndex(e => new { e.DisplayOrder, e.Status }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartProduct>().HasIndex(e => new { e.PartCategoryId, e.Status }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<UserVehicle>().HasIndex(e => new { e.UserId, e.Status }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<OdometerHistory>().HasIndex(e => new { e.UserVehicleId, e.RecordedDate }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<MaintenanceReminder>().HasIndex(e => new { e.VehiclePartTrackingId, e.Level }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<MaintenanceRecord>().HasIndex(e => new { e.UserVehicleId, e.ServiceDate }).HasFilter("\"DeletedAt\" IS NULL");

            // =============== QUERY FILTERS (SOFT DELETE) ===============
            modelBuilder.Entity<VehicleType>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleBrand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleModel>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehicleVariant>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<PartCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<PartProduct>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<DefaultMaintenanceSchedule>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<OdometerHistory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<VehiclePartTracking>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceReminder>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceRecord>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceRecordItem>().HasQueryFilter(e => e.DeletedAt == null);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleType>().HasData(VehicleDataSeeder.GetVehicleTypes());
            modelBuilder.Entity<VehicleBrand>().HasData(VehicleDataSeeder.GetVehicleBrands());
            modelBuilder.Entity<VehicleModel>().HasData(VehicleDataSeeder.GetVehicleModels());
            modelBuilder.Entity<VehicleVariant>().HasData(VehicleDataSeeder.GetVehicleVariants());
            modelBuilder.Entity<PartCategory>().HasData(VehicleDataSeeder.GetPartCategories());
            modelBuilder.Entity<DefaultMaintenanceSchedule>().HasData(VehicleDataSeeder.GetDefaultMaintenanceSchedules());
        }
    }
}
