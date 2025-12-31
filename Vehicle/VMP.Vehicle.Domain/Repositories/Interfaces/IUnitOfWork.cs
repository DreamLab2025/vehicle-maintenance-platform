using VMP.Common.Databases.UnitOfWork;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IVehicleTypeRepository VehicleTypes { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleModelRepository VehicleModels { get; }
        IUserVehicleRepository UserVehicles { get; }
        IConsumableItemRepository ConsumableItems { get; }
        IMaintenanceActivityRepository MaintenanceActivities { get; }
        IOdometerHistoryRepository OdometerHistories { get; }
    }
}
