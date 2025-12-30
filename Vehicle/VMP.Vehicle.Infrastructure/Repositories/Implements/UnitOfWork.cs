using VMP.Common.Databases.UnitOfWork;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class UnitOfWork : BaseUnitOfWork<VehicleDbContext>, IUnitOfWork
    {
        private IVehicleTypeRepository? _vehicleTypes;
        private IVehicleBrandRepository? _vehicleBrands;
        private IVehicleModelRepository? _vehicleModels;
        private IUserVehicleRepository? _userVehicles;
        private IConsumableItemRepository? _consumableItems;
        private IMaintenanceActivityRepository? _maintenanceActivities;

        public UnitOfWork(VehicleDbContext context) : base(context)
        {
        }

        public IVehicleTypeRepository VehicleTypes =>
            _vehicleTypes ??= new VehicleTypeRepository(Context);

        public IVehicleBrandRepository VehicleBrands =>
            _vehicleBrands ??= new VehicleBrandRepository(Context);

        public IVehicleModelRepository VehicleModels =>
            _vehicleModels ??= new VehicleModelRepository(Context);

        public IUserVehicleRepository UserVehicles =>
            _userVehicles ??= new UserVehicleRepository(Context);

        public IConsumableItemRepository ConsumableItems =>
            _consumableItems ??= new ConsumableItemRepository(Context);

        public IMaintenanceActivityRepository MaintenanceActivities =>
            _maintenanceActivities ??= new MaintenanceActivityRepository(Context);
    }
}
