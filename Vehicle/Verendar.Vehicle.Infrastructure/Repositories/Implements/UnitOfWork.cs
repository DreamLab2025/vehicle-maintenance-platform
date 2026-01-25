using Verendar.Common.Databases.UnitOfWork;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class UnitOfWork : BaseUnitOfWork<VehicleDbContext>, IUnitOfWork
    {
        // Vehicle Catalog
        private IVehicleTypeRepository? _vehicleTypes;
        private IVehicleBrandRepository? _vehicleBrands;
        private IVehicleModelRepository? _vehicleModels;
        private IVehicleVariantRepository? _vehicleVariants;

        // Part Catalog
        private IPartCategoryRepository? _partCategories;
        private IPartProductRepository? _partProducts;

        // Maintenance Schedule
        private IDefaultMaintenanceScheduleRepository? _defaultMaintenanceSchedules;

        // User Vehicle & Tracking
        private IUserVehicleRepository? _userVehicles;
        private IOdometerHistoryRepository? _odometerHistories;
        private IVehiclePartTrackingRepository? _vehiclePartTrackings;
        private IMaintenanceReminderRepository? _maintenanceReminders;

        // Maintenance History
        private IMaintenanceRecordRepository? _maintenanceRecords;
        private IMaintenanceRecordItemRepository? _maintenanceRecordItems;

        public UnitOfWork(VehicleDbContext context) : base(context)
        {
        }

        // Vehicle Catalog
        public IVehicleTypeRepository VehicleTypes =>
            _vehicleTypes ??= new VehicleTypeRepository(Context);

        public IVehicleBrandRepository VehicleBrands =>
            _vehicleBrands ??= new VehicleBrandRepository(Context);

        public IVehicleModelRepository VehicleModels =>
            _vehicleModels ??= new VehicleModelRepository(Context);

        public IVehicleVariantRepository VehicleVariants =>
            _vehicleVariants ??= new VehicleVariantRepository(Context);

        // Part Catalog
        public IPartCategoryRepository PartCategories =>
            _partCategories ??= new PartCategoryRepository(Context);

        public IPartProductRepository PartProducts =>
            _partProducts ??= new PartProductRepository(Context);

        // Maintenance Schedule
        public IDefaultMaintenanceScheduleRepository DefaultMaintenanceSchedules =>
            _defaultMaintenanceSchedules ??= new DefaultMaintenanceScheduleRepository(Context);

        // User Vehicle & Tracking
        public IUserVehicleRepository UserVehicles =>
            _userVehicles ??= new UserVehicleRepository(Context);

        public IOdometerHistoryRepository OdometerHistories =>
            _odometerHistories ??= new OdometerHistoryRepository(Context);

        public IVehiclePartTrackingRepository VehiclePartTrackings =>
            _vehiclePartTrackings ??= new VehiclePartTrackingRepository(Context);

        public IMaintenanceReminderRepository MaintenanceReminders =>
            _maintenanceReminders ??= new MaintenanceReminderRepository(Context);

        // Maintenance History
        public IMaintenanceRecordRepository MaintenanceRecords =>
            _maintenanceRecords ??= new MaintenanceRecordRepository(Context);

        public IMaintenanceRecordItemRepository MaintenanceRecordItems =>
            _maintenanceRecordItems ??= new MaintenanceRecordItemRepository(Context);
    }
}
