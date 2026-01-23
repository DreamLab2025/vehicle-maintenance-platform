using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IVehicleTypeRepository VehicleTypes { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleTypeBrandRepository VehicleTypeBrands { get; }
        IVehicleModelRepository VehicleModels { get; }
        IVehicleVariantRepository VehicleVariants { get; }
        IUserVehicleRepository UserVehicles { get; }
        IVehiclePartRepository VehicleParts { get; }
        IVehiclePartCategoryRepository VehiclePartCategories { get; }
        IOilRepository Oils { get; }
        IMaintenanceActivityRepository MaintenanceActivities { get; }
        IOdometerHistoryRepository OdometerHistories { get; }
    }
}
