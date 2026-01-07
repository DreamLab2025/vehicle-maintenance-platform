using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Mappings
{
    public static class ModelImageMappings
    {
        public static ModelImage ToEntity(this ModelImageRequest request)
        {
            return new ModelImage
            {
                VehicleModelId = request.VehicleModelId,
                Color = request.Color,
                HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000",
                ImageUrl = request.ImageUrl
            };
        }

        public static ModelImageResponse ToResponse(this ModelImage entity)
        {
            return new ModelImageResponse
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

        public static void UpdateEntity(this ModelImage entity, ModelImageUpdateRequest request)
        {
            entity.Color = request.Color;
            entity.ImageUrl = request.ImageUrl;
            entity.HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000";
        }
    }
}
