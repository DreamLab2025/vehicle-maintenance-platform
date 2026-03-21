using Verendar.Common.Databases.Base;
using Verendar.Common.Jwt;

namespace Verendar.Vehicle.Infrastructure.Data
{
    public class VehicleDbContext(DbContextOptions<VehicleDbContext> options, ICurrentUserService? currentUserService = null) : BaseDbContext(options, currentUserService)
    {

        // Vehicle Catalog
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<Brand> VehicleBrands { get; set; } = null!;
        public DbSet<Model> VehicleModels { get; set; } = null!;
        public DbSet<Variant> VehicleVariants { get; set; } = null!;

        // Part Catalog
        public DbSet<PartCategory> PartCategories { get; set; } = null!;
        public DbSet<PartProduct> PartProducts { get; set; } = null!;

        // Maintenance Schedule
        public DbSet<DefaultMaintenanceSchedule> DefaultMaintenanceSchedules { get; set; } = null!;

        // User Vehicle & Tracking
        public DbSet<UserVehicle> UserVehicles { get; set; } = null!;
        public DbSet<OdometerHistory> OdometerHistories { get; set; } = null!;
        public DbSet<PartTracking> VehiclePartTrackings { get; set; } = null!;
        public DbSet<TrackingCycle> TrackingCycles { get; set; } = null!;
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
            modelBuilder.Entity<Brand>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<Model>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartCategory>().HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<DefaultMaintenanceSchedule>().HasIndex(e => new { e.VehicleModelId, e.PartCategoryId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartTracking>().HasIndex(e => new { e.UserVehicleId, e.PartCategoryId, e.InstanceIdentifier }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

            // Performance indexes
            modelBuilder.Entity<Model>().HasIndex(e => e.VehicleBrandId).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartCategory>().HasIndex(e => e.DisplayOrder).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<PartProduct>().HasIndex(e => e.PartCategoryId).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<UserVehicle>().HasIndex(e => e.UserId).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<OdometerHistory>().HasIndex(e => new { e.UserVehicleId, e.RecordedDate }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<MaintenanceReminder>().HasIndex(e => new { e.TrackingCycleId, e.Level }).HasFilter("\"DeletedAt\" IS NULL");
            modelBuilder.Entity<MaintenanceRecord>().HasIndex(e => new { e.UserVehicleId, e.ServiceDate }).HasFilter("\"DeletedAt\" IS NULL");

            // =============== QUERY FILTERS (SOFT DELETE) ===============
            modelBuilder.Entity<VehicleType>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Brand>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Model>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Variant>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<PartCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<PartProduct>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<DefaultMaintenanceSchedule>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<OdometerHistory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<PartTracking>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<TrackingCycle>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceReminder>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceRecord>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<MaintenanceRecordItem>().HasQueryFilter(e => e.DeletedAt == null);
        }
    }
}
