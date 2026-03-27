using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        // Vehicle Catalog
        ITypeRepository Types { get; }
        IBrandRepository Brands { get; }
        IModelRepository Models { get; }
        IVariantRepository Variants { get; }

        // Part Catalog
        IPartCategoryRepository PartCategories { get; }
        IPartProductRepository PartProducts { get; }

        // Maintenance Schedule
        IDefaultScheduleRepository DefaultSchedules { get; }

        // User Vehicle & Tracking
        IUserVehicleRepository UserVehicles { get; }
        IOdometerHistoryRepository OdometerHistories { get; }
        IPartTrackingRepository PartTrackings { get; }
        ITrackingCycleRepository TrackingCycles { get; }
        IMaintenanceReminderRepository MaintenanceReminders { get; }

        // Maintenance History
        IMaintenanceRecordRepository MaintenanceRecords { get; }
        IMaintenanceRecordItemRepository MaintenanceRecordItems { get; }

        // Booking Maintenance Proposals
        IMaintenanceProposalRepository MaintenanceProposals { get; }
    }
}
