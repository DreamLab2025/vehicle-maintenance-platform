using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class BrandMappings
    {
        public static VehicleBrand ToEntity(this BrandRequest request)
        {
            return new VehicleBrand
            {
                VehicleTypeId = request.VehicleTypeId,
                Name = request.Name,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                SupportPhone = request.SupportPhone
            };
        }

        public static BrandResponse ToResponse(this VehicleBrand entity)
        {
            return new BrandResponse
            {
                Id = entity.Id,
                VehicleTypeId = entity.VehicleTypeId,
                VehicleTypeName = entity.VehicleType?.Name ?? string.Empty,
                Name = entity.Name,
                LogoUrl = entity.LogoUrl,
                Website = entity.Website,
                SupportPhone = entity.SupportPhone,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this VehicleBrand entity, BrandRequest request)
        {
            entity.VehicleTypeId = request.VehicleTypeId;
            entity.Name = request.Name;
            entity.LogoUrl = request.LogoUrl;
            entity.Website = request.Website;
            entity.SupportPhone = request.SupportPhone;
        }
    }
}
