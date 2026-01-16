using Verender.Common.Databases.Implements;
using Verender.Vehicle.Domain.Entities;
using Verender.Vehicle.Domain.Repositories.Interfaces;
using Verender.Vehicle.Infrastructure.Data;

namespace Verender.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceActivityRepository : PostgresRepository<MaintenanceActivity>, IMaintenanceActivityRepository
    {
        public MaintenanceActivityRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
