using Verendar.Garage.Application.Dtos;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Application.Mappings;

public static class GarageMappings
{
    public static GarageEntity ToEntity(this GarageRequest request, Guid ownerId)
    {
        return new GarageEntity
        {
            OwnerId = ownerId,
            BusinessName = request.BusinessName,
            ShortName = request.ShortName,
            TaxCode = request.TaxCode,
            LogoUrl = request.LogoUrl
        };
    }

    public static GarageResponse ToResponse(this GarageEntity entity)
    {
        return new GarageResponse
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            BusinessName = entity.BusinessName,
            Slug = entity.Slug,
            ShortName = entity.ShortName,
            TaxCode = entity.TaxCode,
            LogoUrl = entity.LogoUrl,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
