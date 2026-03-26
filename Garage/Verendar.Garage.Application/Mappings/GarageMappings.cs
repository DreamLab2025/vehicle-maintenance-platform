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

    public static void UpdateFromRequest(this GarageEntity entity, GarageRequest request)
    {
        entity.BusinessName = request.BusinessName;
        entity.ShortName = request.ShortName;
        entity.TaxCode = request.TaxCode;
        entity.LogoUrl = request.LogoUrl;
    }

    public static GarageDetailResponse ToDetailResponse(this GarageEntity entity)
    {
        return new GarageDetailResponse
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
            UpdatedAt = entity.UpdatedAt,
            BranchCount = entity.Branches.Count,
            Branches = entity.Branches
                .Where(b => b.DeletedAt == null)
                .Select(b => b.ToSummaryResponse())
                .ToList()
        };
    }
}
