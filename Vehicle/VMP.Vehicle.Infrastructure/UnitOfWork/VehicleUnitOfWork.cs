using VMP.Common.Databases.UnitOfWork;
using VMP.Vehicle.Infrastructure.Data;
using VMP.Vehicle.Infrastructure.Repositories.Implements;
using VMP.Vehicle.Infrastructure.Repositories.Interfaces;

namespace VMP.Vehicle.Infrastructure.UnitOfWork
{
    public class VehicleUnitOfWork : BaseUnitOfWork<VehicleDbContext>, IVehicleUnitOfWork
    {
        private IVehicleTypeRepository? _vehicleTypes;
        private IVehicleBrandRepository? _vehicleBrands;
        private IVehicleModelRepository? _vehicleModels;
        private IUserVehicleRepository? _userVehicles;
        private IConsumableItemRepository? _consumableItems;
        private IMaintenanceActivityRepository? _maintenanceActivities;

        public VehicleUnitOfWork(VehicleDbContext context) : base(context)
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
