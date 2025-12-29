using VMP.Vehicle.Infrastructure.Repositories.Interfaces;

namespace VMP.Vehicle.Infrastructure.UnitOfWork
{
    public interface IVehicleUnitOfWork : Common.UnitOfWork.IUnitOfWork
    {
        IVehicleTypeRepository VehicleTypes { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleModelRepository VehicleModels { get; }
        IUserVehicleRepository UserVehicles { get; }
        IConsumableItemRepository ConsumableItems { get; }
        IMaintenanceActivityRepository MaintenanceActivities { get; }
    }
}
