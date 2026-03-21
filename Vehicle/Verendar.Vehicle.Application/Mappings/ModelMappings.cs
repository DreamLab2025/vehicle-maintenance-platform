namespace Verendar.Vehicle.Application.Mappings
{
    public static class ModelMappings
    {
        public static Model ToEntity(this ModelRequest request)
        {
            return new Model
            {
                Name = request.Name,
                Code = request.Code ?? string.Empty,
                VehicleBrandId = request.BrandId,
                ManufactureYear = request.ReleaseYear,
                FuelType = request.FuelType,
                TransmissionType = request.TransmissionType,
                EngineDisplacement = request.EngineDisplacement,
                EngineCapacity = request.EngineCapacity
            };
        }

        public static ModelResponse ToModelResponse(this Model entity)
        {
            return new ModelResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                BrandId = entity.VehicleBrandId,
                BrandName = entity.Brand?.Name ?? string.Empty,
                TypeId = entity.Brand?.VehicleTypeId ?? Guid.Empty,
                TypeName = entity.Brand?.VehicleType?.Name ?? string.Empty,
                ReleaseYear = entity.ManufactureYear,
                FuelType = entity.FuelType,
                FuelTypeName = entity.FuelType.HasValue ? GetFuelTypeName(entity.FuelType.Value) : string.Empty,
                TransmissionType = entity.TransmissionType,
                TransmissionTypeName = entity.TransmissionType.HasValue ? GetTransmissionTypeName(entity.TransmissionType.Value) : string.Empty,
                EngineDisplacementDisplay = entity.EngineDisplacement.HasValue ? $"{entity.EngineDisplacement} cc" : null,
                EngineCapacity = entity.EngineCapacity,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static ModelResponseWithVariants ToModelResponseWithVariants(this Model entity)
        {
            return new ModelResponseWithVariants
            {
                Id = entity.Id,
                Name = entity.Name,
                BrandId = entity.VehicleBrandId,
                BrandName = entity.Brand?.Name ?? string.Empty,
                TypeId = entity.Brand?.VehicleTypeId ?? Guid.Empty,
                TypeName = entity.Brand?.VehicleType?.Name ?? string.Empty,
                ReleaseYear = entity.ManufactureYear,
                FuelType = entity.FuelType,
                FuelTypeName = entity.FuelType.HasValue ? GetFuelTypeName(entity.FuelType.Value) : string.Empty,
                TransmissionType = entity.TransmissionType,
                TransmissionTypeName = entity.TransmissionType.HasValue ? GetTransmissionTypeName(entity.TransmissionType.Value) : string.Empty,
                Variants = entity.Variants?.Select(mi => mi.ToResponse()).ToList() ?? [],
                EngineDisplacementDisplay = entity.EngineDisplacement.HasValue ? $"{entity.EngineDisplacement} cc" : null,
                EngineCapacity = entity.EngineCapacity,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this Model entity, ModelRequest request)
        {
            entity.Name = request.Name;
            entity.Code = request.Code ?? string.Empty;
            entity.VehicleBrandId = request.BrandId;
            entity.ManufactureYear = request.ReleaseYear;
            entity.FuelType = request.FuelType;
            entity.TransmissionType = request.TransmissionType;
            entity.EngineDisplacement = request.EngineDisplacement;
            entity.EngineCapacity = request.EngineCapacity;
        }

        private static string GetFuelTypeName(VehicleFuelType fuelType)
        {
            return fuelType switch
            {
                VehicleFuelType.Gasoline => "Xăng",
                VehicleFuelType.Diesel => "Dầu Diesel",
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
                _ => "Không xác định"
            };
        }
    }
}
