using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Application.Mappings
{
    public static class VariantMappings
    {
        public static VehicleVariant ToEntity(this VehicleVariantRequest request)
        {
            return new VehicleVariant
            {
                VehicleModelId = request.VehicleModelId,
                Color = request.Color,
                HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000",
                ImageUrl = request.ImageUrl
            };
        }

        public static VehicleVariantResponse ToResponse(this VehicleVariant entity)
        {
            return new VehicleVariantResponse
            {
                Id = entity.Id,
                VehicleModelId = entity.VehicleModelId,
                Color = entity.Color,
                HexCode = entity.HexCode,
                ImageUrl = entity.ImageUrl,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static UserVehicleVariantResponse ToUserVehicleVariantResponse(this VehicleVariant entity)
        {
            return new UserVehicleVariantResponse
            {
                Id = entity.Id,
                VehicleModelId = entity.VehicleModelId,
                Color = entity.Color,
                HexCode = entity.HexCode,
                ImageUrl = entity.ImageUrl,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Model = entity.VehicleModel?.ToModelResponse() ?? new ModelResponse()
            };
        }

        public static void UpdateEntity(this VehicleVariant entity, VehicleVariantUpdateRequest request)
        {
            entity.Color = request.Color;
            entity.ImageUrl = request.ImageUrl;
            entity.HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000";
        }
    }
}
