using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class PartCategoryMappings
    {
        public static PartCategory ToEntity(this PartCategoryRequest request)
        {
            return new PartCategory
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                IconUrl = request.IconUrl,
                DisplayOrder = request.DisplayOrder,
                RequiresOdometerTracking = request.RequiresOdometerTracking,
                RequiresTimeTracking = request.RequiresTimeTracking,
                AllowsMultipleInstances = request.AllowsMultipleInstances
            };
        }

        public static PartCategoryResponse ToResponse(this PartCategory entity)
        {
            return new PartCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                IconUrl = entity.IconUrl,
                DisplayOrder = entity.DisplayOrder,
                Status = entity.Status,
                RequiresOdometerTracking = entity.RequiresOdometerTracking,
                RequiresTimeTracking = entity.RequiresTimeTracking,
                AllowsMultipleInstances = entity.AllowsMultipleInstances,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this PartCategory entity, PartCategoryRequest request)
        {
            entity.Name = request.Name;
            entity.Code = request.Code;
            entity.Description = request.Description;
            entity.IconUrl = request.IconUrl;
            entity.DisplayOrder = request.DisplayOrder;
            entity.RequiresOdometerTracking = request.RequiresOdometerTracking;
            entity.RequiresTimeTracking = request.RequiresTimeTracking;
            entity.AllowsMultipleInstances = request.AllowsMultipleInstances;
        }
    }

    public static class PartProductMappings
    {
        public static PartProduct ToEntity(this PartProductRequest request)
        {
            return new PartProduct
            {
                PartCategoryId = request.PartCategoryId,
                Name = request.Name,
                Brand = request.Brand,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ReferencePrice = request.ReferencePrice,
                RecommendedKmInterval = request.RecommendedKmInterval,
                RecommendedMonthsInterval = request.RecommendedMonthsInterval
            };
        }

        public static PartProductResponse ToResponse(this PartProduct entity)
        {
            return new PartProductResponse
            {
                Id = entity.Id,
                PartCategoryId = entity.PartCategoryId,
                PartCategoryName = entity.Category?.Name ?? string.Empty,
                Name = entity.Name,
                Brand = entity.Brand,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                ReferencePrice = entity.ReferencePrice,
                RecommendedKmInterval = entity.RecommendedKmInterval,
                RecommendedMonthsInterval = entity.RecommendedMonthsInterval,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void UpdateEntity(this PartProduct entity, PartProductRequest request)
        {
            entity.PartCategoryId = request.PartCategoryId;
            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Description = request.Description;
            entity.ImageUrl = request.ImageUrl;
            entity.ReferencePrice = request.ReferencePrice;
            entity.RecommendedKmInterval = request.RecommendedKmInterval;
            entity.RecommendedMonthsInterval = request.RecommendedMonthsInterval;
        }
    }
}
