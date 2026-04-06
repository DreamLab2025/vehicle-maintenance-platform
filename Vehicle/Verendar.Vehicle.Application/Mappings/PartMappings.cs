namespace Verendar.Vehicle.Application.Mappings
{
    public static class PartCategoryMappings
    {
        public static PartCategory ToEntity(this PartCategoryRequest request)
        {
            return new PartCategory
            {
                Name = request.Name,
                Slug = string.Empty,
                Description = request.Description,
                IconUrl = request.IconUrl,
                IconMediaFileId = request.IconMediaFileId,
                DisplayOrder = request.DisplayOrder,
                RequiresOdometerTracking = request.RequiresOdometerTracking,
                RequiresTimeTracking = request.RequiresTimeTracking,
                AllowsMultipleInstances = request.AllowsMultipleInstances,
                IdentificationSigns = request.IdentificationSigns,
                ConsequencesIfNotHandled = request.ConsequencesIfNotHandled
            };
        }

        public static PartCategoryResponse ToResponse(this PartCategory entity)
        {
            return new PartCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                IconUrl = entity.IconUrl,
                IconMediaFileId = entity.IconMediaFileId,
                DisplayOrder = entity.DisplayOrder,
                RequiresOdometerTracking = entity.RequiresOdometerTracking,
                RequiresTimeTracking = entity.RequiresTimeTracking,
                AllowsMultipleInstances = entity.AllowsMultipleInstances,
                IdentificationSigns = entity.IdentificationSigns,
                ConsequencesIfNotHandled = entity.ConsequencesIfNotHandled,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static PartCategorySummary ToSummary(this PartCategory entity)
        {
            return new PartCategorySummary
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                IconUrl = entity.IconUrl,
                IconMediaFileId = entity.IconMediaFileId,
                DisplayOrder = entity.DisplayOrder,
            };
        }

        public static void UpdateFromRequest(this PartCategory entity, PartCategoryRequest request)
        {
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.IconUrl = request.IconUrl;
            entity.IconMediaFileId = request.IconMediaFileId;
            entity.DisplayOrder = request.DisplayOrder;
            entity.RequiresOdometerTracking = request.RequiresOdometerTracking;
            entity.RequiresTimeTracking = request.RequiresTimeTracking;
            entity.AllowsMultipleInstances = request.AllowsMultipleInstances;
            entity.IdentificationSigns = request.IdentificationSigns;
            entity.ConsequencesIfNotHandled = request.ConsequencesIfNotHandled;
        }
    }
}
