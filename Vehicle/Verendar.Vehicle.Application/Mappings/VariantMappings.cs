namespace Verendar.Vehicle.Application.Mappings
{
    public static class VariantMappings
    {
        public static Variant ToEntity(this VariantRequest request)
        {
            return new Variant
            {
                VehicleModelId = request.VehicleModelId,
                Color = request.Color,
                HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000",
                ImageUrl = request.ImageUrl,
                ImageMediaFileId = request.ImageMediaFileId
            };
        }

        public static VariantResponse ToResponse(this Variant entity)
        {
            return new VariantResponse
            {
                Id = entity.Id,
                VehicleModelId = entity.VehicleModelId,
                Color = entity.Color,
                HexCode = entity.HexCode,
                ImageUrl = entity.ImageUrl,
                ImageMediaFileId = entity.ImageMediaFileId,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static UserVariantResponse ToUserVariantResponse(this Variant entity)
        {
            return new UserVariantResponse
            {
                Id = entity.Id,
                VehicleModelId = entity.VehicleModelId,
                Color = entity.Color,
                HexCode = entity.HexCode,
                ImageUrl = entity.ImageUrl,
                ImageMediaFileId = entity.ImageMediaFileId,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Model = entity.VehicleModel?.ToModelResponse() ?? new ModelResponse()
            };
        }

        public static void UpdateEntity(this Variant entity, VariantUpdateRequest request)
        {
            entity.Color = request.Color;
            entity.ImageUrl = request.ImageUrl;
            entity.ImageMediaFileId = request.ImageMediaFileId;
            entity.HexCode = ColorCode.IsHex(request.HexCode) ? request.HexCode : "#000000";
        }
    }
}
