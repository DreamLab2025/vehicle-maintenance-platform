using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class ServiceCategoryMappings
{
    public static ServiceCategory ToEntity(this CreateServiceCategoryRequest request) =>
        new()
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            IconUrl = request.IconUrl,
            DisplayOrder = request.DisplayOrder
        };

    public static void UpdateFromRequest(this ServiceCategory entity, UpdateServiceCategoryRequest request)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IconUrl = request.IconUrl;
        entity.DisplayOrder = request.DisplayOrder;
    }

    public static ServiceCategoryResponse ToResponse(this ServiceCategory entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            Description = entity.Description,
            IconUrl = entity.IconUrl,
            DisplayOrder = entity.DisplayOrder,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
