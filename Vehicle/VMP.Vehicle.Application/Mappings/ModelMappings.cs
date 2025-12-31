using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Mappings
{
    public static class ModelMappings
    {
        public static VehicleModel ToEntity(this ModelRequest request)
        {
            return new VehicleModel
            {
                Name = request.Name,
                BrandId = request.BrandId,
                TypeId = request.TypeId,
                ReleaseYear = request.ReleaseYear,
                FuelType = request.FuelType,
                ImageUrl = request.ImageUrl,
                OilCapacity = request.OilCapacity,
                TireSizeFront = request.TireSizeFront,
                TireSizeRear = request.TireSizeRear
            };
        }

        public static ModelResponse ToResponse(this VehicleModel entity)
        {
            return new ModelResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                BrandId = entity.BrandId,
                BrandName = entity.Brand?.Name ?? string.Empty,
                TypeId = entity.TypeId,
                TypeName = entity.Type?.Name ?? string.Empty,
                ReleaseYear = entity.ReleaseYear,
                FuelType = entity.FuelType,
                FuelTypeName = GetFuelTypeName(entity.FuelType),
                ImageUrl = entity.ImageUrl,
                OilCapacity = entity.OilCapacity,
                TireSizeFront = entity.TireSizeFront,
                TireSizeRear = entity.TireSizeRear,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this VehicleModel entity, ModelRequest request)
        {
            entity.Name = request.Name;
            entity.BrandId = request.BrandId;
            entity.TypeId = request.TypeId;
            entity.ReleaseYear = request.ReleaseYear;
            entity.FuelType = request.FuelType;
            entity.ImageUrl = request.ImageUrl;
            entity.OilCapacity = request.OilCapacity;
            entity.TireSizeFront = request.TireSizeFront;
            entity.TireSizeRear = request.TireSizeRear;
        }

        private static string GetFuelTypeName(VehicleFuelType fuelType)
        {
            return fuelType switch
            {
                VehicleFuelType.Gasoline => "Xăng",
                VehicleFuelType.Diesel => "Dầu Diesel",
                VehicleFuelType.Electric => "Điện",
                VehicleFuelType.Hybrid => "Hybrid",
                _ => "Không xác định"
            };
        }
    }
}
