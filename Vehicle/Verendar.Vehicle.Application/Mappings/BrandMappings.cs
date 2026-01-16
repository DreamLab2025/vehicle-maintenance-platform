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
                Name = request.Name,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                SupportPhone = request.SupportPhone
            };
        }

        public static BrandResponse ToResponse(this VehicleBrand entity, List<string>? typeNames = null)
        {
            return new BrandResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                VehicleTypeNames = typeNames ?? entity.VehicleTypeBrands?
                    .Where(vtb => vtb.VehicleType != null)
                    .Select(vtb => vtb.VehicleType.Name)
                    .ToList() ?? new List<string>(),
                LogoUrl = entity.LogoUrl,
                Website = entity.Website,
                SupportPhone = entity.SupportPhone,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this VehicleBrand entity, BrandRequest request)
        {
            entity.Name = request.Name;
            entity.LogoUrl = request.LogoUrl;
            entity.Website = request.Website;
            entity.SupportPhone = request.SupportPhone;
        }
    }
}
