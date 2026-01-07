using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class OdometerHistoryRepository : PostgresRepository<OdometerHistory>, IOdometerHistoryRepository
    {
        public OdometerHistoryRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
