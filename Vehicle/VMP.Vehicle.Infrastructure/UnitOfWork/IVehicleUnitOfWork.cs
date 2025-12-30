using VMP.Common.Databases.UnitOfWork;
using VMP.Vehicle.Infrastructure.Repositories.Interfaces;

namespace VMP.Vehicle.Infrastructure.UnitOfWork
{
    public interface IVehicleUnitOfWork : IBaseUnitOfWork
    {
        IVehicleTypeRepository VehicleTypes { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleModelRepository VehicleModels { get; }
        IUserVehicleRepository UserVehicles { get; }
        IConsumableItemRepository ConsumableItems { get; }
        IMaintenanceActivityRepository MaintenanceActivities { get; }
    }
}
