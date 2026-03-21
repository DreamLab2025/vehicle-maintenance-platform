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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleDbContext).Assembly);
        }
    }
}
