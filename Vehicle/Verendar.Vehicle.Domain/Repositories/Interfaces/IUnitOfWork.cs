using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        // Vehicle Catalog
        IVehicleTypeRepository VehicleTypes { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleTypeBrandRepository VehicleTypeBrands { get; }
        IVehicleModelRepository VehicleModels { get; }
        IVehicleVariantRepository VehicleVariants { get; }

        // Part Catalog
        IPartCategoryRepository PartCategories { get; }
        IPartProductRepository PartProducts { get; }

        // Maintenance Schedule
        IDefaultMaintenanceScheduleRepository DefaultMaintenanceSchedules { get; }

        // User Vehicle & Tracking
        IUserVehicleRepository UserVehicles { get; }
        IOdometerHistoryRepository OdometerHistories { get; }
        IVehiclePartTrackingRepository VehiclePartTrackings { get; }
        IMaintenanceReminderRepository MaintenanceReminders { get; }

        // Maintenance History
        IMaintenanceRecordRepository MaintenanceRecords { get; }
        IMaintenanceRecordItemRepository MaintenanceRecordItems { get; }
    }
}
