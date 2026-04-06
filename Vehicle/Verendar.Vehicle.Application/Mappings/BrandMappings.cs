namespace Verendar.Vehicle.Application.Mappings
{
    public static class BrandMappings
    {
        public static Brand ToEntity(this BrandRequest request)
        {
            return new Brand
            {
                VehicleTypeId = request.VehicleTypeId,
                Name = request.Name,
                LogoUrl = request.LogoUrl,
                LogoMediaFileId = request.LogoMediaFileId,
                Website = request.Website,
                SupportPhone = request.SupportPhone
            };
        }

        public static BrandResponse ToResponse(this Brand entity)
        {
            return new BrandResponse
            {
                Id = entity.Id,
                VehicleTypeId = entity.VehicleTypeId,
                VehicleTypeName = entity.VehicleType?.Name ?? string.Empty,
                Name = entity.Name,
                Slug = entity.Slug,
                LogoUrl = entity.LogoUrl,
                LogoMediaFileId = entity.LogoMediaFileId,
                Website = entity.Website,
                SupportPhone = entity.SupportPhone,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static BrandSummary ToSummary(this Brand entity)
        {
            return new BrandSummary
            {
                Id = entity.Id,
                VehicleTypeId = entity.VehicleTypeId,
                VehicleTypeName = entity.VehicleType?.Name ?? string.Empty,
                Name = entity.Name,
                Slug = entity.Slug,
                LogoUrl = entity.LogoUrl,
                LogoMediaFileId = entity.LogoMediaFileId
            };
        }

        public static void UpdateFromRequest(this Brand entity, BrandRequest request)
        {
            entity.VehicleTypeId = request.VehicleTypeId;
            entity.Name = request.Name;
            entity.LogoUrl = request.LogoUrl;
            entity.LogoMediaFileId = request.LogoMediaFileId;
            entity.Website = request.Website;
            entity.SupportPhone = request.SupportPhone;
        }
    }
}
