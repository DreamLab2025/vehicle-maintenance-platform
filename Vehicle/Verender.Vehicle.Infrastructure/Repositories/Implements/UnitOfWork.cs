using Verender.Common.Databases.UnitOfWork;
using Verender.Vehicle.Domain.Repositories.Interfaces;
using Verender.Vehicle.Infrastructure.Data;

namespace Verender.Vehicle.Infrastructure.Repositories.Implements
{
    public class UnitOfWork : BaseUnitOfWork<VehicleDbContext>, IUnitOfWork
    {
        private IVehicleTypeRepository? _vehicleTypes;
        private IVehicleBrandRepository? _vehicleBrands;
        private IVehicleTypeBrandRepository? _vehicleTypeBrands;
        private IVehicleModelRepository? _vehicleModels;
        private IVehicleVariantRepository? _vehicleVariants;
        private IUserVehicleRepository? _userVehicles;
        private IConsumableItemRepository? _consumableItems;
        private IMaintenanceActivityRepository? _maintenanceActivities;
        private IOdometerHistoryRepository? _odometerHistories;

        public UnitOfWork(VehicleDbContext context) : base(context)
        {
        }

        public IVehicleTypeRepository VehicleTypes =>
            _vehicleTypes ??= new VehicleTypeRepository(Context);

        public IVehicleBrandRepository VehicleBrands =>
            _vehicleBrands ??= new VehicleBrandRepository(Context);

        public IVehicleTypeBrandRepository VehicleTypeBrands =>
            _vehicleTypeBrands ??= new VehicleTypeBrandRepository(Context);

        public IVehicleModelRepository VehicleModels =>
            _vehicleModels ??= new VehicleModelRepository(Context);

        public IVehicleVariantRepository VehicleVariants =>
            _vehicleVariants ??= new VehicleVariantRepository(Context);

        public IUserVehicleRepository UserVehicles =>
            _userVehicles ??= new UserVehicleRepository(Context);

        public IConsumableItemRepository ConsumableItems =>
            _consumableItems ??= new ConsumableItemRepository(Context);

        public IMaintenanceActivityRepository MaintenanceActivities =>
            _maintenanceActivities ??= new MaintenanceActivityRepository(Context);

        public IOdometerHistoryRepository OdometerHistories =>
            _odometerHistories ??= new OdometerHistoryRepository(Context);
    }
}
