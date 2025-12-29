using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Infrastructure.Data;
using VMP.Vehicle.Infrastructure.Repositories.Interfaces;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class ConsumableItemRepository : PostgresRepository<ConsumableItem>, IConsumableItemRepository
    {
        public ConsumableItemRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
