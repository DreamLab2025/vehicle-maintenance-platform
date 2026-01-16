using Verender.Vehicle.Application.Dtos;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Application.Mappings
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
                TransmissionType = request.TransmissionType,
                EngineDisplacement = request.EngineDisplacement,
                EngineCapacity = request.EngineCapacity,
                OilCapacity = request.OilCapacity,
                TireSizeFront = request.TireSizeFront,
                TireSizeRear = request.TireSizeRear
            };
        }

        public static ModelResponse ToModelResponse(this VehicleModel entity)
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
                TransmissionType = entity.TransmissionType,
                TransmissionTypeName = GetTransmissionTypeName(entity.TransmissionType),
                EngineDisplacementDisplay = entity.EngineDisplacement.HasValue ? $"{entity.EngineDisplacement} cc" : null,
                EngineCapacity = entity.EngineCapacity,
                OilCapacity = entity.OilCapacity,
                TireSizeFront = entity.TireSizeFront,
                TireSizeRear = entity.TireSizeRear,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static ModelResponseWithVariants ToModelResponseWithVariants(this VehicleModel entity)
        {
            return new ModelResponseWithVariants
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
                TransmissionType = entity.TransmissionType,
                TransmissionTypeName = GetTransmissionTypeName(entity.TransmissionType),
                Variants = entity.VehicleVariants?.Select(mi => mi.ToResponse()).ToList() ?? [],
                EngineDisplacementDisplay = entity.EngineDisplacement.HasValue ? $"{entity.EngineDisplacement} cc" : null,
                EngineCapacity = entity.EngineCapacity,
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
            entity.TransmissionType = request.TransmissionType;
            entity.EngineDisplacement = request.EngineDisplacement;
            entity.EngineCapacity = request.EngineCapacity;
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

        private static string GetTransmissionTypeName(VehicleTransmissionType transmissionType)
        {
            return transmissionType switch
            {
                VehicleTransmissionType.Manual => "Xe số",
                VehicleTransmissionType.Automatic => "Tay ga",
                VehicleTransmissionType.Sport => "Xe côn",
                VehicleTransmissionType.ManualCar => "Số sàn",
                VehicleTransmissionType.AutomaticCar => "Số tự động",
                VehicleTransmissionType.Electric => "Điện",
                _ => "Không xác định"
            };
        }
    }
}
