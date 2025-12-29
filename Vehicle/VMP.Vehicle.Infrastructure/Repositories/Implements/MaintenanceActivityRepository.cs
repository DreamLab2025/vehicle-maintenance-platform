using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Infrastructure.Data;
using VMP.Vehicle.Infrastructure.Repositories.Interfaces;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceActivityRepository : PostgresRepository<MaintenanceActivity>, IMaintenanceActivityRepository
    {
        public MaintenanceActivityRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
