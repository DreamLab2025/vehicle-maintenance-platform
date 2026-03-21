using Verendar.Common.Databases.UnitOfWork;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class UnitOfWork(VehicleDbContext context) : BaseUnitOfWork<VehicleDbContext>(context), IUnitOfWork
    {
        // Vehicle Catalog
        private ITypeRepository? _types;
        private IBrandRepository? _brands;
        private IModelRepository? _models;
        private IVariantRepository? _variants;

        // Part Catalog
        private IPartCategoryRepository? _partCategories;
        private IPartProductRepository? _partProducts;

        // Maintenance Schedule
        private IDefaultScheduleRepository? _defaultSchedules;

        // User Vehicle & Tracking
        private IUserVehicleRepository? _userVehicles;
        private IOdometerHistoryRepository? _odometerHistories;
        private IPartTrackingRepository? _partTrackings;
        private IMaintenanceReminderRepository? _maintenanceReminders;

        // Maintenance History
        private IMaintenanceRecordRepository? _maintenanceRecords;
        private IMaintenanceRecordItemRepository? _maintenanceRecordItems;

        // Vehicle Catalog
        public ITypeRepository Types =>
            _types ??= new TypeRepository(Context);

        public IBrandRepository Brands =>
            _brands ??= new BrandRepository(Context);

        public IModelRepository Models =>
            _models ??= new ModelRepository(Context);

        public IVariantRepository Variants =>
            _variants ??= new VariantRepository(Context);

        // Part Catalog
        public IPartCategoryRepository PartCategories =>
            _partCategories ??= new PartCategoryRepository(Context);

        public IPartProductRepository PartProducts =>
            _partProducts ??= new PartProductRepository(Context);

        // Maintenance Schedule
        public IDefaultScheduleRepository DefaultSchedules =>
            _defaultSchedules ??= new DefaultScheduleRepository(Context);

        // User Vehicle & Tracking
        public IUserVehicleRepository UserVehicles =>
            _userVehicles ??= new UserVehicleRepository(Context);

        public IOdometerHistoryRepository OdometerHistories =>
            _odometerHistories ??= new OdometerHistoryRepository(Context);

        public IPartTrackingRepository PartTrackings =>
            _partTrackings ??= new PartTrackingRepository(Context);

        public IMaintenanceReminderRepository MaintenanceReminders =>
            _maintenanceReminders ??= new MaintenanceReminderRepository(Context);

        // Maintenance History
        public IMaintenanceRecordRepository MaintenanceRecords =>
            _maintenanceRecords ??= new MaintenanceRecordRepository(Context);

        public IMaintenanceRecordItemRepository MaintenanceRecordItems =>
            _maintenanceRecordItems ??= new MaintenanceRecordItemRepository(Context);
    }
}
