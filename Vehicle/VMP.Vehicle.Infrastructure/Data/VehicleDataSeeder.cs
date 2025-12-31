using VMP.Common.Databases.Base;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Infrastructure.Data
{
    public static class VehicleDataSeeder
    {
        public static List<VehicleType> GetVehicleTypes()
        {
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var systemUserId = Guid.Empty;

            return new List<VehicleType>
            {
                new VehicleType
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Xe máy",
                    Description = "Xe máy hai bánh, bao gồm xe số, xe tay ga, xe côn tay",
                    Status = EntityStatus.Active,
                    CreatedAt = fixedDate,
                    CreatedBy = systemUserId
                },
                new VehicleType
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Xe ô tô",
                    Description = "Xe ô tô 4 bánh trở lên, bao gồm sedan, SUV, hatchback, MPV",
                    Status = EntityStatus.Active,
                    CreatedAt = fixedDate,
                    CreatedBy = systemUserId
                },
                new VehicleType
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Xe điện",
                    Description = "Xe chạy bằng động cơ điện, bao gồm xe máy điện và ô tô điện",
                    Status = EntityStatus.Active,
                    CreatedAt = fixedDate,
                    CreatedBy = systemUserId
                }
            };
        }
    }
}