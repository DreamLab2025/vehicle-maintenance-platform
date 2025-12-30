using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleBrandRepository : PostgresRepository<VehicleBrand>, IVehicleBrandRepository
    {
        public VehicleBrandRepository(VehicleDbContext context) : base(context)
        {
        }
    }
}
