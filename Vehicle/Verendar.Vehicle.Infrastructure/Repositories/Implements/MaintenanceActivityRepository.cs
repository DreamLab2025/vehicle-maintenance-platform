using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceActivityRepository : PostgresRepository<MaintenanceActivity>, IMaintenanceActivityRepository
    {
        public MaintenanceActivityRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
