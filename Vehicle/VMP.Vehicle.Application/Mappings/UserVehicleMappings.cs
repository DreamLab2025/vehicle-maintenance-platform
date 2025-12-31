using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Mappings
{
    public static class UserVehicleMappings
    {
        public static UserVehicle ToEntity(this UserVehicleRequest request, Guid userId)
        {
            return new UserVehicle
            {
                UserId = userId,
                VehicleModelId = request.VehicleModelId,
                LicensePlate = request.LicensePlate,
                Nickname = request.Nickname,
                VinNumber = request.VinNumber,
                PurchaseDate = request.PurchaseDate,
                CurrentOdometer = request.CurrentOdometer,
                LastOdometerUpdateAt = DateTime.UtcNow,
                AverageKmPerDay = 0,
                LastCalculatedDate = null
            };
        }

        public static UserVehicleResponse ToResponse(this UserVehicle entity)
        {
            return new UserVehicleResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                VehicleModelId = entity.VehicleModelId,
                VehicleModelName = entity.VehicleModel?.Name ?? string.Empty,
                BrandName = entity.VehicleModel?.Brand?.Name ?? string.Empty,
                TypeName = entity.VehicleModel?.Type?.Name ?? string.Empty,
                LicensePlate = entity.LicensePlate,
                Nickname = entity.Nickname,
                VinNumber = entity.VinNumber,
                PurchaseDate = entity.PurchaseDate,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdateAt = entity.LastOdometerUpdateAt,
                AverageKmPerDay = entity.AverageKmPerDay,
                LastCalculatedDate = entity.LastCalculatedDate,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static UserVehicleDetailResponse ToDetailResponse(this UserVehicle entity, int totalMaintenanceActivities = 0, DateTime? lastMaintenanceDate = null)
        {
            var daysSincePurchase = (DateTime.UtcNow - entity.PurchaseDate).Days;
            
            return new UserVehicleDetailResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                VehicleModelId = entity.VehicleModelId,
                VehicleModelName = entity.VehicleModel?.Name ?? string.Empty,
                BrandName = entity.VehicleModel?.Brand?.Name ?? string.Empty,
                TypeName = entity.VehicleModel?.Type?.Name ?? string.Empty,
                LicensePlate = entity.LicensePlate,
                Nickname = entity.Nickname,
                VinNumber = entity.VinNumber,
                PurchaseDate = entity.PurchaseDate,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdateAt = entity.LastOdometerUpdateAt,
                AverageKmPerDay = entity.AverageKmPerDay,
                LastCalculatedDate = entity.LastCalculatedDate,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                TotalMaintenanceActivities = totalMaintenanceActivities,
                LastMaintenanceDate = lastMaintenanceDate,
                DaysSincePurchase = daysSincePurchase,
                TotalKmDriven = entity.CurrentOdometer
            };
        }

        public static void UpdateEntity(this UserVehicle entity, UserVehicleRequest request)
        {
            entity.VehicleModelId = request.VehicleModelId;
            entity.LicensePlate = request.LicensePlate;
            entity.Nickname = request.Nickname;
            entity.VinNumber = request.VinNumber;
            entity.PurchaseDate = request.PurchaseDate;
        }

        public static void UpdateOdometer(this UserVehicle entity, int newOdometer)
        {
            var oldOdometer = entity.CurrentOdometer;
            entity.CurrentOdometer = newOdometer;
            entity.LastOdometerUpdateAt = DateTime.UtcNow;

            // Calculate average km per day
            var daysSincePurchase = (DateTime.UtcNow - entity.PurchaseDate).Days;
            if (daysSincePurchase > 0)
            {
                entity.AverageKmPerDay = Math.Round((decimal)newOdometer / daysSincePurchase, 2);
            }
            entity.LastCalculatedDate = DateTime.UtcNow;
        }
    }
}
