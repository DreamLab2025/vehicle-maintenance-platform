using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class ConsumableItemRepository : PostgresRepository<ConsumableItem>, IConsumableItemRepository
    {
        public ConsumableItemRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
